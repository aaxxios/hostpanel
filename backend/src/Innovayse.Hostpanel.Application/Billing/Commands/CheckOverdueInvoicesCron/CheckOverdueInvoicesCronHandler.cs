using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Billing.Commands.CheckOverdueInvoicesCron;
/// <summary>
/// Handles <see cref="CheckOverdueInvoicesCronCommand"/> by marking all unpaid invoices
/// past their due date as overdue. Re-schedules itself for the next day after completion.
/// </summary>
public sealed class CheckOverdueInvoicesCronHandler
{
    /// <summary>Hour (UTC) at which this job runs daily.</summary>
    private const int CronHour = 7;

    /// <summary>The module's data context, for the unpaid invoices past their due date.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Message bus, for re-scheduling the job.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<CheckOverdueInvoicesCronHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context, for the unpaid invoices past their due date.</param>
    /// <param name="uow">Unit of work.</param>
    /// <param name="bus">Message bus, for re-scheduling the job.</param>
    /// <param name="logger">Structured logger.</param>
    public CheckOverdueInvoicesCronHandler(
        IHostpanelDbContext context,
        IUnitOfWork uow,
        IMessageBus bus,
        ILogger<CheckOverdueInvoicesCronHandler> logger)
    {
        _context = context;
        _uow = uow;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Queries all unpaid invoices past their due date and transitions them to Overdue status.
    /// Each marked invoice raises an <see cref="Innovayse.Hostpanel.Domain.Billing.Events.InvoiceOverdueEvent"/>.
    /// </summary>
    /// <param name="cmd">The scheduled command (marker, no payload).</param>
    /// <param name="ct">Cancellation token.</param>
    public Task HandleAsync(CheckOverdueInvoicesCronCommand cmd, CancellationToken ct)
    {
        _ = cmd;

        return RecurringJob.RunAsync(
            nameof(CheckOverdueInvoicesCronCommand), () => RunAsync(ct), ScheduleNextAsync, _logger, ct);
    }

    /// <summary>One pass of the job; <see cref="HandleAsync"/> runs it through <see cref="RecurringJob"/>.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    private async Task RunAsync(CancellationToken ct)
    {
        // Was IInvoiceRepository.ListUnpaidOverdueAsync. Tracked, not AsNoTracking: each matching
        // invoice has MarkOverdue() called on it below and is saved in this same pass.
        var overdueInvoices = await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Unpaid && i.DueDate < DateTimeOffset.UtcNow)
            .ToListAsync(ct);

        foreach (var invoice in overdueInvoices)
        {
            invoice.MarkOverdue();
        }

        if (overdueInvoices.Count > 0)
        {
            await _uow.SaveChangesAsync(ct);
        }

        _logger.LogInformation(
            "CheckOverdueInvoicesCron marked {Count} invoices as overdue.", overdueInvoices.Count);
    }

    /// <summary>Schedules the next pass.</summary>
    /// <returns>A task.</returns>
    private async Task ScheduleNextAsync()
    {
        var nextRun = DailySchedule.NextOccurrence(CronHour, DateTimeOffset.UtcNow);
        await _bus.ScheduleAsync(new CheckOverdueInvoicesCronCommand(), nextRun);
        _logger.LogInformation("CheckOverdueInvoicesCronCommand re-scheduled for {NextRun:u}.", nextRun);
    }
}

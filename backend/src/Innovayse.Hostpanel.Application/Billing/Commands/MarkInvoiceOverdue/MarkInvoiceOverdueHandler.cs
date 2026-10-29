using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.MarkInvoiceOverdue;

/// <summary>Marks an unpaid invoice as overdue (called by a scheduled job or admin).</summary>
public sealed class MarkInvoiceOverdueHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The clock the current moment is read from.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">The clock the current moment is read from.</param>
    public MarkInvoiceOverdueHandler(IHostpanelDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The mark-overdue command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>INVOICE_NOT_FOUND</c> when the id names no invoice. A status that cannot
    /// transition (Paid or Cancelled) still throws: see
    /// <see cref="Domain.Billing.Invoice.MarkOverdue"/>.
    /// </returns>
    public async Task<Result> HandleAsync(MarkInvoiceOverdueCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.Id == cmd.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        invoice.MarkOverdue(_clock.UtcNow);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}

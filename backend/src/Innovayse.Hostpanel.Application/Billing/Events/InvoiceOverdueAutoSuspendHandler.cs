using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Commands.SuspendService;
using Innovayse.Hostpanel.Domain.Billing.Events;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Billing.Events;

/// <summary>
/// Handles <see cref="InvoiceOverdueEvent"/> by automatically suspending active client services
/// that do not have the auto-suspend override flag set.
/// </summary>
public sealed class InvoiceOverdueAutoSuspendHandler
{
    /// <summary>The module's data context, for the client's services.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Message bus, for dispatching the per-service suspend command.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<InvoiceOverdueAutoSuspendHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context, for the client's services.</param>
    /// <param name="bus">Message bus, for dispatching the per-service suspend command.</param>
    /// <param name="logger">Structured logger.</param>
    public InvoiceOverdueAutoSuspendHandler(
        IHostpanelDbContext context, IMessageBus bus, ILogger<InvoiceOverdueAutoSuspendHandler> logger)
    {
        _context = context;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Suspends all eligible active services for the client whose invoice became overdue.
    /// Services with <see cref="ClientService.OverrideAutoSuspend"/> set to <see langword="true"/> are skipped.
    /// </summary>
    /// <param name="evt">The overdue invoice event containing the client ID.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(InvoiceOverdueEvent evt, CancellationToken ct)
    {
        // Was IClientServiceRepository.ListByClientAsync -- read-only here: each eligible service
        // is suspended through SuspendServiceCommand below, not by mutating this loaded copy.
        var services = await _context.ClientServices
            .AsNoTracking()
            .Where(s => s.ClientId == evt.ClientId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        var eligible = services
            .Where(s => s.Status == ServiceStatus.Active && !s.OverrideAutoSuspend)
            .ToList();

        foreach (var service in eligible)
        {
            var outcome = await _bus.InvokeAsync<Result>(new SuspendServiceCommand(service.Id), ct);
            if (outcome.IsFailure)
            {
                _logger.LogWarning(
                    "Auto-suspend refused for service {ServiceId} ({Code}).", service.Id, outcome.ErrorCode);
            }
        }

        _logger.LogInformation(
            "InvoiceOverdueAutoSuspend suspended {Count} services for client {ClientId} (invoice {InvoiceId}).",
            eligible.Count, evt.ClientId, evt.InvoiceId);
    }
}

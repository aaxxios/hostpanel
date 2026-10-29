using Innovayse.Hostpanel.Application.Billing.Commands.CancelInvoice;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Clients.Events;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Billing.Events;

/// <summary>
/// Settles the billing of a closed account: every renewal invoice still <see cref="InvoiceStatus.Unpaid"/>
/// is cancelled through <see cref="CancelInvoiceCommand"/> (which hands applied credit back to the
/// balance), so nothing is collected for a term the closed account will never use. An invoice that is
/// already overdue stays: it is a debt on record, and cancelling it would also let the overdue
/// suspension of its service lift. A non-notification handler, and safe as one: cancelling is
/// idempotent, and a failure is logged for staff.
/// </summary>
public sealed class ClientClosedInvoicesHandler
{
    /// <summary>The module's data context, to find the client's unpaid renewal invoices.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Dispatches the cancellations.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Records an invoice that could not be cancelled, so staff can settle it by hand.</summary>
    private readonly ILogger<ClientClosedInvoicesHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context, to find the client's unpaid renewal invoices.</param>
    /// <param name="bus">Dispatches the cancellations.</param>
    /// <param name="logger">Records an invoice that could not be cancelled.</param>
    public ClientClosedInvoicesHandler(IHostpanelDbContext context, IMessageBus bus, ILogger<ClientClosedInvoicesHandler> logger)
    {
        _context = context;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>Cancels the closed client's unpaid renewal invoices.</summary>
    /// <param name="evt">The closing.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    public async Task HandleAsync(ClientClosedEvent evt, CancellationToken ct)
    {
        try
        {
            var invoiceIds = await _context.Invoices.AsNoTracking()
                .Where(i => i.ClientId == evt.ClientId
                            && i.Status == InvoiceStatus.Unpaid
                            && i.Items.Any(l => l.RenewalPeriodStart != null))
                .Select(i => i.Id)
                .ToListAsync(ct);

            foreach (var invoiceId in invoiceIds)
            {
                var outcome = await _bus.InvokeAsync<Result>(new CancelInvoiceCommand(invoiceId), ct);
                if (outcome.IsFailure)
                {
                    _logger.LogWarning(
                        "Cancelling renewal invoice {InvoiceId} of closed client {ClientId} was refused with {ErrorCode}.",
                        invoiceId, evt.ClientId, outcome.ErrorCode);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Wolverine combines every handler of one message into a single chain, so a throw here
            // would also stop the other handlers of the closing.
            _logger.LogError(ex, "Cancelling the renewal invoices of closed client {ClientId} failed.", evt.ClientId);
        }
    }
}

using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Audit;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.DeleteInvoice;

/// <summary>Permanently deletes an invoice regardless of its current status.</summary>
public sealed class DeleteInvoiceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Current request context, providing the acting admin's identity and IP for the audit trail.</summary>
    private readonly ICurrentUserService _ctx;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="ctx">Current request context providing admin identity and IP.</param>
    public DeleteInvoiceHandler(IHostpanelDbContext context, ICurrentUserService ctx)
    {
        _context = context;
        _ctx = ctx;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The delete command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>INVOICE_NOT_FOUND</c> when the id names no invoice.</returns>
    public async Task<Result> HandleAsync(DeleteInvoiceCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        var clientId = invoice.ClientId;
        var invoiceId = invoice.Id;

        _context.Invoices.Remove(invoice);
        _context.ActivityLogs.Add(ActivityLog.Create(
            clientId,
            $"Deleted Invoice - Invoice ID: {invoiceId}",
            _ctx.UserId, _ctx.UserName, _ctx.Email, _ctx.IpAddress));
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}

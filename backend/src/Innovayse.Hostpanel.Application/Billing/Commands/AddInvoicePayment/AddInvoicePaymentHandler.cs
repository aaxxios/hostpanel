using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Audit;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.AddInvoicePayment;

/// <summary>
/// Records a manual payment transaction against an invoice
/// and creates a corresponding client-level transaction ledger entry.
/// </summary>
public sealed class AddInvoicePaymentHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Who is acting, for the audit trail's admin identity and IP.</summary>
    private readonly ICurrentUserService _ctx;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="ctx">Who is acting, for the audit trail's admin identity and IP.</param>
    public AddInvoicePaymentHandler(IHostpanelDbContext context, ICurrentUserService ctx)
    {
        _context = context;
        _ctx = ctx;
    }

    /// <summary>Handles <see cref="AddInvoicePaymentCommand"/>.</summary>
    /// <param name="cmd">The add payment command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>INVOICE_NOT_FOUND</c> when the id names no invoice.</returns>
    public async Task<Result> HandleAsync(AddInvoicePaymentCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        invoice.AddPayment(cmd.Date, cmd.Gateway, cmd.TransactionId, cmd.Amount, cmd.Fees, cmd.Notes);

        _context.Transactions.Add(Transaction.CreateInvoicePayment(
            invoice, cmd.Date, cmd.Gateway, cmd.TransactionId, cmd.Amount, cmd.Fees));

        // Was IActivityLogRepository.Add.
        _context.ActivityLogs.Add(ActivityLog.Create(
            invoice.ClientId,
            $"Added Invoice Payment - Invoice ID: {invoice.Id}",
            _ctx.UserId, _ctx.UserName, _ctx.Email, _ctx.IpAddress));

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}

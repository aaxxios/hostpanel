using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.ApplyInvoiceCredit;

/// <summary>
/// Moves credit from the client's balance onto an invoice, reducing the amount owed. Never more
/// than the balance holds or the invoice still owes.
/// </summary>
public sealed class ApplyInvoiceCreditHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Takes the credit off the client's balance in the same save.</summary>
    private readonly IClientCredit _clientCredit;

    /// <summary>Stamps the moment a credit that covers the rest settles the invoice.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clientCredit">Takes the credit off the client's balance in the same save.</param>
    /// <param name="clock">Stamps the moment a credit that covers the rest settles the invoice.</param>
    public ApplyInvoiceCreditHandler(IHostpanelDbContext context, IClientCredit clientCredit, IClock clock)
    {
        _context = context;
        _clientCredit = clientCredit;
        _clock = clock;
    }

    /// <summary>Handles <see cref="ApplyInvoiceCreditCommand"/>.</summary>
    /// <param name="cmd">The apply credit command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>INVOICE_NOT_FOUND</c> when the id names no invoice or <c>CLIENT_NOT_FOUND</c>
    /// when its client is gone, or <c>CLIENT_CREDIT_INSUFFICIENT</c> when the balance is short; nothing
    /// is saved on a refusal. From the domain: <c>INVOICE_CREDIT_EXCEEDS_DUE</c> when the invoice owes less, and
    /// <c>INVOICE_NOT_EDITABLE</c> when it no longer takes credit.
    /// </returns>
    public async Task<Result> HandleAsync(ApplyInvoiceCreditCommand cmd, CancellationToken ct)
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

        var applied = await _clientCredit.ApplyFromBalanceAsync(invoice, cmd.Amount, ct);
        if (applied.IsFailure)
        {
            return applied;
        }

        // Credit that covers what a partial payment left owes nothing more: the invoice is settled in
        // this save, so it does not stay open, go overdue and suspend a client who has paid.
        invoice.SettleIfCovered(_clock.UtcNow);

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}

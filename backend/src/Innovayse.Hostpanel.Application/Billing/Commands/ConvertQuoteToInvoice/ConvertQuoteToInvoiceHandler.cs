using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Billing.Commands.ConvertQuoteToInvoice;

/// <summary>
/// Converts a quote into a draft invoice by copying its line items. The quote stage is set to
/// <see cref="QuoteStage.Accepted"/> after conversion.
/// </summary>
public sealed class ConvertQuoteToInvoiceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the currency the quoted client is billed in, stamped on the invoice.</summary>
    private readonly IPayerCurrencyResolver _payerCurrency;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="payerCurrency">The currency the quoted client is billed in, stamped on the invoice.</param>
    public ConvertQuoteToInvoiceHandler(IHostpanelDbContext context, IPayerCurrencyResolver payerCurrency)
    {
        _context = context;
        _payerCurrency = payerCurrency;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The convert command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the ID of the newly created draft invoice, or <c>QUOTE_NOT_FOUND</c> when the
    /// quote does not exist. The quote's own stage invariant still throws
    /// — see <see cref="Quote.Accept"/>.
    /// </returns>
    public async Task<Result<int>> HandleAsync(ConvertQuoteToInvoiceCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var quote = await _context.Quotes
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == cmd.QuoteId, ct);
        if (quote is null)
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.QUOTE_NOT_FOUND);
        }

        var itemData = quote.GetInvoiceItemData();
        var currency = (await _payerCurrency.ForClientAsync(quote.ClientId, ct)).Code;
        var invoice = Invoice.CreateDraft(quote.ClientId, quote.ExpiryDate, currency);

        foreach (var (description, unitPrice, quantity) in itemData)
        {
            invoice.AddItem(description, unitPrice, quantity);
        }

        _context.Invoices.Add(invoice);
        quote.Accept();

        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(invoice.Id);
    }
}

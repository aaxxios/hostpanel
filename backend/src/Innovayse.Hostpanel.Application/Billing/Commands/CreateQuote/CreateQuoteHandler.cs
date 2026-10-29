using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Billing.Commands.CreateQuote;

/// <summary>Creates a new quote with the provided line items and persists it.</summary>
public sealed class CreateQuoteHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the currency the quoted client is billed in, stamped on the quote.</summary>
    private readonly IPayerCurrencyResolver _payerCurrency;

    /// <summary>Wolverine bus the owning modules' existence queries are dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="payerCurrency">The currency the quoted client is billed in, stamped on the quote.</param>
    /// <param name="bus">Wolverine bus the owning modules' existence queries are dispatched through.</param>
    public CreateQuoteHandler(IHostpanelDbContext context, IPayerCurrencyResolver payerCurrency, IMessageBus bus)
    {
        _context = context;
        _payerCurrency = payerCurrency;
        _bus = bus;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The create quote command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the newly created quote ID, or <c>CLIENT_NOT_FOUND</c> when the id names no client.</returns>
    public async Task<Result<int>> HandleAsync(CreateQuoteCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        if (!await _bus.ClientExistsAsync(cmd.ClientId, ct))
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        // The quote is priced in the currency its client is billed in: their own, else the base.
        var currency = (await _payerCurrency.ForClientAsync(cmd.ClientId, ct)).Code;
        var quote = Quote.Create(
            cmd.ClientId, cmd.Subject, cmd.ExpiryDate, currency, cmd.Notes, cmd.ProposalText, cmd.CustomerNotes, cmd.AdminNotes);

        foreach (var item in cmd.Items)
        {
            quote.AddItem(item.Description, item.UnitPrice, item.Quantity, item.DiscountPercent, item.Taxed);
        }

        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(quote.Id);
    }
}

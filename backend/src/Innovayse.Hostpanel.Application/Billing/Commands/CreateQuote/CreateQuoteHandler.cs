using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Billing.Commands.CreateQuote;

/// <summary>Creates a new quote with the provided line items and persists it.</summary>
public sealed class CreateQuoteHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CreateQuoteHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The create quote command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the newly created quote ID. This use case refuses nothing itself.</returns>
    public async Task<Result<int>> HandleAsync(CreateQuoteCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var quote = Quote.Create(
            cmd.ClientId, cmd.Subject, cmd.ExpiryDate, cmd.Notes, cmd.ProposalText, cmd.CustomerNotes, cmd.AdminNotes);

        foreach (var item in cmd.Items)
        {
            quote.AddItem(item.Description, item.UnitPrice, item.Quantity, item.DiscountPercent, item.Taxed);
        }

        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(quote.Id);
    }
}

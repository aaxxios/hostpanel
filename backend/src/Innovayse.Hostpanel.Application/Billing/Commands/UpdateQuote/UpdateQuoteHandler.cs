using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.UpdateQuote;

/// <summary>Updates an existing quote's details and line items (add, update, delete).</summary>
public sealed class UpdateQuoteHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateQuoteHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The update quote command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>QUOTE_NOT_FOUND</c> when the id names no quote. Item-level and stage
    /// invariants still throw: see <see cref="Domain.Billing.Quote.UpdateItem"/> and
    /// <see cref="Domain.Billing.Quote.RemoveItem"/>.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateQuoteCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var quote = await _context.Quotes
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == cmd.QuoteId, ct);
        if (quote is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.QUOTE_NOT_FOUND);
        }

        quote.UpdateDetails(
            cmd.Subject, cmd.Stage, cmd.ValidUntil, cmd.Notes, cmd.ProposalText, cmd.CustomerNotes, cmd.AdminNotes);

        foreach (var entry in cmd.Items)
        {
            if (entry.IsDeleted && entry.Id.HasValue)
            {
                quote.RemoveItem(entry.Id.Value);
            }
            else if (entry.Id.HasValue)
            {
                quote.UpdateItem(
                    entry.Id.Value, entry.Description, entry.UnitPrice, entry.Quantity,
                    entry.DiscountPercent, entry.Taxed);
            }
            else
            {
                quote.AddItem(entry.Description, entry.UnitPrice, entry.Quantity, entry.DiscountPercent, entry.Taxed);
            }
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}

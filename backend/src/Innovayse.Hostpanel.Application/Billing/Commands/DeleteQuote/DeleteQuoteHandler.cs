using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.DeleteQuote;

/// <summary>Permanently deletes a quote.</summary>
public sealed class DeleteQuoteHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public DeleteQuoteHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The delete command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>QUOTE_NOT_FOUND</c> when the id names no quote.</returns>
    public async Task<Result> HandleAsync(DeleteQuoteCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var quote = await _context.Quotes.FirstOrDefaultAsync(x => x.Id == cmd.QuoteId, ct);
        if (quote is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.QUOTE_NOT_FOUND);
        }

        _context.Quotes.Remove(quote);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}

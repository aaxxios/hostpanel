using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.DuplicateQuote;

/// <summary>Creates a draft copy of an existing quote.</summary>
public sealed class DuplicateQuoteHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public DuplicateQuoteHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The duplicate command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the ID of the newly created draft quote, or <c>QUOTE_NOT_FOUND</c> when the
    /// source quote does not exist.
    /// </returns>
    public async Task<Result<int>> HandleAsync(DuplicateQuoteCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var source = await _context.Quotes
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == cmd.QuoteId, ct);
        if (source is null)
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.QUOTE_NOT_FOUND);
        }

        var copy = source.Duplicate();
        _context.Quotes.Add(copy);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(copy.Id);
    }
}

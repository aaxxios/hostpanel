using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListPredefinedReplies;

/// <summary>
/// Returns all predefined replies, optionally filtered by category, mapped to
/// <see cref="PredefinedReplyDto"/>.
/// </summary>
/// <remarks>
/// The category join is an inner join, on purpose: a reply whose category row no longer exists is
/// not returned.
/// </remarks>
public sealed class ListPredefinedRepliesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListPredefinedRepliesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The optional category filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every matching reply, by name. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<PredefinedReplyDto>>> HandleAsync(
        ListPredefinedRepliesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Ordered before the join is projected into the DTO: EF cannot translate an ORDER BY
        // applied to a property read off a constructed record, only to a column.
        var replies = await _context.PredefinedReplies
            .AsNoTracking()
            .Where(r => query.CategoryId == null || r.CategoryId == query.CategoryId.Value)
            .OrderBy(r => r.Name)
            .Join(
                _context.PredefinedReplyCategories.AsNoTracking(),
                r => r.CategoryId,
                c => c.Id,
                (r, c) => new PredefinedReplyDto(r.Id, r.Name, r.Content, r.CategoryId, c.Name))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<PredefinedReplyDto>>(replies);
    }
}

using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Faqs.Common;
using Innovayse.Hostpanel.Application.Faqs.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Faqs.Queries.ListFaqs;

/// <summary>Loads every question with its translations and maps them to <see cref="FaqAdminDto"/>.</summary>
public sealed class ListFaqsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public ListFaqsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The query; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every question, ordered by sort order ascending. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<FaqAdminDto>>> HandleAsync(ListFaqsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await _context.Faqs.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync(ct);
        return Result<IReadOnlyList<FaqAdminDto>>.Success(rows.Select(x => x.ToAdminDto()).ToList());
    }
}

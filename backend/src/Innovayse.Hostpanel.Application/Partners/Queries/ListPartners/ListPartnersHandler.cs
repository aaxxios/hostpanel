using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Partners.Common;
using Innovayse.Hostpanel.Application.Partners.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Partners.Queries.ListPartners;

/// <summary>Loads every partner with its translations and maps them to <see cref="PartnerAdminDto"/>.</summary>
public sealed class ListPartnersHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public ListPartnersHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The query; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every partner, ordered by sort order ascending. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<PartnerAdminDto>>> HandleAsync(ListPartnersQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await _context.Partners.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync(ct);
        return Result<IReadOnlyList<PartnerAdminDto>>.Success(rows.Select(x => x.ToAdminDto()).ToList());
    }
}

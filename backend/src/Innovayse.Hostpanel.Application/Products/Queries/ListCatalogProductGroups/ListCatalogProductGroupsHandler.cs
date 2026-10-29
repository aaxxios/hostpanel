using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Products.Common;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Products.Queries.ListCatalogProductGroups;

/// <summary>
/// Returns the storefront catalogue's product groups: active groups only, each with the count of
/// its active products, without the fields only staff should see.
/// </summary>
public sealed class ListCatalogProductGroupsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public ListCatalogProductGroupsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="qry">The query; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The active groups, by name. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<CatalogProductGroupDto>>> HandleAsync(
        ListCatalogProductGroupsQuery qry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(qry);

        var groups = await _context.ProductGroups
            .AsNoTracking()
            .Where(g => g.IsActive)
            .OrderBy(g => g.Name)
            .Select(g => new CatalogProductGroupDto(
                g.Id, g.Name, g.Description, g.Products.Count(p => p.Status == ProductStatus.Active)))
            .ToListAsync(ct);

        return Result<IReadOnlyList<CatalogProductGroupDto>>.Success(groups);
    }
}

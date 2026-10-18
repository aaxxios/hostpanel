using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Loads every configured product for matching a plugin record's product name against. Extracted
/// from <c>MigrationPullWorker</c> (A6 Task 6) so <see cref="ServicesImporter"/>,
/// <see cref="ProductsImporter"/> and <see cref="OrdersImporter"/> share the one lookup instead of
/// each querying it separately.
/// </summary>
/// <param name="context">The module's data context.</param>
public sealed class MigrationProductLookup(IHostpanelDbContext context)
{
    /// <summary>
    /// Loads every product with its per-currency prices, mirroring the dissolved
    /// <c>IProductRepository.ListAsync(null, false, ct)</c> (no group filter, all statuses).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every product, ordered by name, with <c>Prices</c> included.</returns>
    public async Task<IReadOnlyList<Product>> LoadAllProductsAsync(CancellationToken ct) =>
        await context.Products
            .AsNoTracking()
            .Include(p => p.Prices)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
}

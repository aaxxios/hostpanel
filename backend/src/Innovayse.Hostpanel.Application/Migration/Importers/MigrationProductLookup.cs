using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Loads every configured product and owns the one rule that says which local product a source product is. It exists so
/// <see cref="ServicesImporter"/>, <see cref="ProductsImporter"/> and <see cref="OrdersImporter"/> share the one lookup
/// and the one matching rule instead of each querying and comparing separately.
/// </summary>
public sealed class MigrationProductLookup
{
    /// <summary>The source plugin dataset the products are read from, the same one <see cref="ProductsImporter"/> imports.</summary>
    private const string ProductsDataSet = "products";

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Pages through the source plugin's products dataset.</summary>
    private readonly MigrationRecordPager _pager;

    /// <summary>Initializes a new instance of the <see cref="MigrationProductLookup"/> class.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="pager">Pages through the source plugin's products dataset.</param>
    public MigrationProductLookup(IHostpanelDbContext context, MigrationRecordPager pager)
    {
        _context = context;
        _pager = pager;
    }

    /// <summary>
    /// Finds the local product a source product name stands for. The products importer creates a product only when no
    /// product here carries its name, so the name is the identity a source product keeps here.
    /// </summary>
    /// <param name="products">The products configured here.</param>
    /// <param name="name">The source product's name.</param>
    /// <returns>The product carrying that name, compared case-insensitively; null when none does.</returns>
    public static Product? FindByName(IReadOnlyList<Product> products, string name) =>
        products.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Loads every product with its per-currency prices, with no group filter and all statuses.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every product, ordered by name, with <c>Prices</c> included.</returns>
    public async Task<IReadOnlyList<Product>> LoadAllProductsAsync(CancellationToken ct) =>
        await _context.Products
            .AsNoTracking()
            .Include(p => p.Prices)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

    /// <summary>
    /// Maps each source product id to the local product imported for it, by reading the source's products dataset and
    /// matching each record the way <see cref="ProductsImporter"/> does. A record that refers to a source product by id
    /// is resolved through this map, never by a name the record itself carries.
    /// </summary>
    /// <param name="job">The migration job whose source is read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The local product per source product id; a source product with no product here is absent.</returns>
    public async Task<IReadOnlyDictionary<int, Product>> LoadSourceProductMapAsync(MigrationJob job, CancellationToken ct)
    {
        var products = await LoadAllProductsAsync(ct);
        var map = new Dictionary<int, Product>();

        await foreach (var page in _pager.PagesAsync<ProductRecord>(job, ProductsDataSet, ct))
        {
            foreach (var record in page)
            {
                if (FindByName(products, record.Name) is { } product)
                {
                    map[record.Id] = product;
                }
            }
        }

        return map;
    }
}

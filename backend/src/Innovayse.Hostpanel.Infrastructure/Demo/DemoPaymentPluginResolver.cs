using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.SDK.Plugins;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Resolves every payment module to the one demo gateway, installed or not. The gateway accepts
/// every currency the shop has, read from the stored currencies so the seed stays the one list.
/// </summary>
public sealed class DemoPaymentPluginResolver : IPaymentPluginResolver
{
    /// <summary>ISO 4217 numeric code of USD, used only when no currency has been stored yet.</summary>
    private const string FallbackNumericCode = "840";

    /// <summary>The store the currencies are read from.</summary>
    private readonly AppDbContext _db;

    /// <summary>Creates the resolver.</summary>
    /// <param name="db">The store holding the seeded currencies.</param>
    public DemoPaymentPluginResolver(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<IPaymentPlugin?> ResolveAsync(string module, CancellationToken ct)
    {
        var currencies = await _db.Currencies
            .AsNoTracking()
            .OrderByDescending(c => c.IsBase)
            .Select(c => c.Numeric)
            .ToListAsync(ct);

        return new DemoPaymentPlugin(currencies.FirstOrDefault() ?? FallbackNumericCode, currencies);
    }
}

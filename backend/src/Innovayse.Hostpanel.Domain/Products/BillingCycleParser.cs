using Innovayse.Hostpanel.Domain.Products.Enums;
namespace Innovayse.Hostpanel.Domain.Products;

/// <summary>
/// Parses the billing-cycle strings the order flow and cart have always sent.
/// </summary>
/// <remarks>
/// Three spellings are in the wild — "monthly", "annual" and "annually" — because the client
/// and the seed data never agreed. They are accepted here, in one place, so no handler has to
/// carry its own switch over them.
/// </remarks>
public static class BillingCycleParser
{
    /// <summary>Parses a cycle string.</summary>
    /// <param name="raw">"monthly", "annual" or "annually", any case.</param>
    /// <returns>The cycle.</returns>
    /// <exception cref="ArgumentException">The string is none of the known spellings.</exception>
    public static BillingCycle Parse(string raw) => raw?.Trim().ToLowerInvariant() switch
    {
        BillingCycleNames.Monthly => BillingCycle.Monthly,
        BillingCycleNames.Annual or BillingCycleNames.Annually => BillingCycle.Annual,
        _ => throw new ArgumentException($"Unknown billing cycle '{raw}'.", nameof(raw)),
    };

    /// <summary>
    /// Reads a stored billing-cycle string without throwing, for code that must skip what it does
    /// not understand (a renewal must never guess a cycle and bill the wrong amount of time).
    /// </summary>
    /// <param name="raw">The stored value, any casing; may be null.</param>
    /// <param name="cycle">The parsed cycle when this returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> when <paramref name="raw"/> names a known cycle.</returns>
    public static bool TryParse(string? raw, out BillingCycle cycle)
    {
        switch (raw?.Trim().ToLowerInvariant())
        {
            case BillingCycleNames.Monthly:
                cycle = BillingCycle.Monthly;
                return true;
            case BillingCycleNames.Annual or BillingCycleNames.Annually:
                cycle = BillingCycle.Annual;
                return true;
            default:
                cycle = default;
                return false;
        }
    }
}

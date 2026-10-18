namespace Innovayse.Hostpanel.Domain.Products;

/// <summary>
/// The billing-cycle spellings the cart, the order flow and the persisted service rows carry.
/// </summary>
/// <remarks>
/// These are wire and database values, so they cannot become an enum member name without
/// changing stored data. They live here once so no handler repeats the literal.
/// </remarks>
public static class BillingCycleNames
{
    /// <summary>A monthly cycle.</summary>
    public const string Monthly = "monthly";

    /// <summary>A yearly cycle, as stored on a client service.</summary>
    public const string Annual = "annual";

    /// <summary>A yearly cycle, the alternative spelling the cart and the domain flows send.</summary>
    public const string Annually = "annually";
}

using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.Stripe;

/// <summary>
/// Converts between the domain's ISO 4217 minor units and the integer amounts Stripe takes and
/// reports. Stripe departs from ISO 4217 for a few currencies, so every amount sent to or read from
/// Stripe goes through here and nowhere else.
/// </summary>
/// <remarks>
/// Stripe treats ISK and UGX as two-decimal currencies (the integer is the amount times 100, and
/// must be a whole multiple of 100), although ISO 4217 gives them no minor unit. For the
/// three-decimal currencies Stripe supports (BHD, JOD, KWD, OMR, TND) the integer is in thousandths
/// but its last digit must be 0, so the amount is effectively rounded to two decimals.
/// </remarks>
public static class StripeAmount
{
    /// <summary>Factor between a whole major unit and the integer Stripe uses for ISK and UGX.</summary>
    private const int TwoDecimalFactor = 100;

    /// <summary>Granularity of a three-decimal currency's amount on Stripe: the last digit must be 0.</summary>
    private const int ThreeDecimalStep = 10;

    /// <summary>Currencies ISO 4217 gives no minor unit but Stripe prices in hundredths.</summary>
    private static readonly HashSet<string> TwoDecimalOnStripe = new(StringComparer.OrdinalIgnoreCase)
    {
        "ISK", "UGX",
    };

    /// <summary>Three-decimal currencies whose Stripe amount must end in 0.</summary>
    private static readonly HashSet<string> ThreeDecimalOnStripe = new(StringComparer.OrdinalIgnoreCase)
    {
        "BHD", "JOD", "KWD", "OMR", "TND",
    };

    /// <summary>Converts a major-unit amount to the integer Stripe expects, rounding half away from zero.</summary>
    /// <param name="amount">The amount in major units.</param>
    /// <param name="currency">The ISO 4217 code, any case.</param>
    /// <returns>The amount in Stripe's units.</returns>
    public static long ToStripe(decimal amount, string currency)
    {
        var minor = CurrencyCode.ToMinorUnits(amount, currency);
        if (TwoDecimalOnStripe.Contains(currency))
        {
            return minor * TwoDecimalFactor;
        }

        if (ThreeDecimalOnStripe.Contains(currency))
        {
            return (long)Math.Round(minor / (decimal)ThreeDecimalStep, MidpointRounding.AwayFromZero) * ThreeDecimalStep;
        }

        return minor;
    }

    /// <summary>Converts an integer amount Stripe reported to a major-unit amount.</summary>
    /// <param name="stripeAmount">The amount in Stripe's units.</param>
    /// <param name="currency">The ISO 4217 code, any case.</param>
    /// <returns>The amount in major units.</returns>
    public static decimal FromStripe(long stripeAmount, string currency) =>
        TwoDecimalOnStripe.Contains(currency)
            ? stripeAmount / (decimal)TwoDecimalFactor
            : CurrencyCode.FromMinorUnits(stripeAmount, currency);

    /// <summary>Converts an integer amount Stripe reported to the domain's ISO 4217 minor units.</summary>
    /// <param name="stripeAmount">The amount in Stripe's units.</param>
    /// <param name="currency">The ISO 4217 code, any case.</param>
    /// <returns>The amount in ISO 4217 minor units, as the settlement compares it.</returns>
    public static long ToMinorUnits(long stripeAmount, string currency) =>
        CurrencyCode.ToMinorUnits(FromStripe(stripeAmount, currency), currency);
}

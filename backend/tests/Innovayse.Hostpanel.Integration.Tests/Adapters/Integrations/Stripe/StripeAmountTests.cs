using System.Globalization;
using Innovayse.Hostpanel.Infrastructure.Integrations.Stripe;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Integrations.Stripe;

/// <summary>
/// <see cref="StripeAmount"/>: the one conversion between the domain's ISO 4217 minor units and the
/// integers Stripe takes and reports, including the currencies where Stripe departs from ISO.
/// </summary>
public sealed class StripeAmountTests
{
    /// <summary>Two-decimal and zero-decimal currencies Stripe treats like ISO 4217 are sent as the plain minor unit.</summary>
    /// <param name="amount">The amount in major units.</param>
    /// <param name="currency">The ISO 4217 code.</param>
    /// <param name="expected">The integer Stripe expects.</param>
    [Theory]
    [InlineData("10.005", "USD", 1001)]
    [InlineData("10.00", "usd", 1000)]
    [InlineData("1500.5", "JPY", 1501)]
    [InlineData("2500", "KRW", 2500)]
    public void ToStripe_SendsTheIsoMinorUnitForOrdinaryCurrencies(string amount, string currency, long expected) =>
        StripeAmount.ToStripe(decimal.Parse(amount, CultureInfo.InvariantCulture), currency).Should().Be(expected);

    /// <summary>ISK and UGX have no ISO minor unit but Stripe prices them in hundredths, so the whole amount is sent times 100.</summary>
    /// <param name="amount">The amount in major units.</param>
    /// <param name="currency">The ISO 4217 code.</param>
    /// <param name="expected">The integer Stripe expects.</param>
    [Theory]
    [InlineData("1500", "ISK", 150000)]
    [InlineData("1500.4", "isk", 150000)]
    [InlineData("2000", "UGX", 200000)]
    public void ToStripe_SendsIskAndUgxTimesOneHundred(string amount, string currency, long expected) =>
        StripeAmount.ToStripe(decimal.Parse(amount, CultureInfo.InvariantCulture), currency).Should().Be(expected);

    /// <summary>Three-decimal currencies are sent in thousandths with the last digit forced to 0, rounding half away from zero.</summary>
    /// <param name="amount">The amount in major units.</param>
    /// <param name="currency">The ISO 4217 code.</param>
    /// <param name="expected">The integer Stripe expects.</param>
    [Theory]
    [InlineData("1.234", "KWD", 1230)]
    [InlineData("1.235", "KWD", 1240)]
    [InlineData("1.005", "BHD", 1010)]
    [InlineData("12.3456", "JOD", 12350)]
    [InlineData("0.004", "OMR", 0)]
    [InlineData("5.000", "TND", 5000)]
    public void ToStripe_EndsThreeDecimalCurrenciesInZero(string amount, string currency, long expected)
    {
        var sent = StripeAmount.ToStripe(decimal.Parse(amount, CultureInfo.InvariantCulture), currency);

        sent.Should().Be(expected);
        (sent % 10).Should().Be(0);
    }

    /// <summary>An amount Stripe reports is read back to major units with the same per-currency rule.</summary>
    /// <param name="stripeAmount">The integer Stripe reported.</param>
    /// <param name="currency">The ISO 4217 code.</param>
    /// <param name="expected">The amount in major units.</param>
    [Theory]
    [InlineData(1001, "usd", "10.01")]
    [InlineData(1501, "jpy", "1501")]
    [InlineData(150000, "isk", "1500")]
    [InlineData(200000, "UGX", "2000")]
    [InlineData(1230, "kwd", "1.230")]
    public void FromStripe_ReadsStripeUnitsBackToMajorUnits(long stripeAmount, string currency, string expected) =>
        StripeAmount.FromStripe(stripeAmount, currency).Should().Be(decimal.Parse(expected, CultureInfo.InvariantCulture));

    /// <summary>An amount Stripe reports is converted to the ISO minor units the settlement compares against an invoice.</summary>
    /// <param name="stripeAmount">The integer Stripe reported.</param>
    /// <param name="currency">The ISO 4217 code.</param>
    /// <param name="expected">The amount in ISO 4217 minor units.</param>
    [Theory]
    [InlineData(1001, "usd", 1001)]
    [InlineData(150000, "isk", 1500)]
    [InlineData(200000, "ugx", 2000)]
    [InlineData(1230, "kwd", 1230)]
    public void ToMinorUnits_ReadsStripeUnitsBackToIsoMinorUnits(long stripeAmount, string currency, long expected) =>
        StripeAmount.ToMinorUnits(stripeAmount, currency).Should().Be(expected);
}

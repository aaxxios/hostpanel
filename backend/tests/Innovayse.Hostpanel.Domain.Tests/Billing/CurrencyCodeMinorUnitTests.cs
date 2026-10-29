using System.Globalization;
using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Domain.Tests.Billing;

/// <summary>
/// <see cref="CurrencyCode.ToMinorUnits"/>: the one major to minor unit conversion every gateway
/// uses, by the currency's ISO 4217 exponent, rounding half away from zero and never truncating.
/// </summary>
public sealed class CurrencyCodeMinorUnitTests
{
    /// <summary>Two-decimal currencies convert to hundredths, rounding an uneven amount half away from zero.</summary>
    /// <param name="amount">The major-unit amount, invariant text so it reaches the test as an exact decimal.</param>
    /// <param name="code">The currency.</param>
    /// <param name="expected">The minor-unit amount.</param>
    [Theory]
    [InlineData("25", "USD", 2500)]
    [InlineData("10.005", "usd", 1001)]
    [InlineData("10.004", "EUR", 1000)]
    [InlineData("19.999", "AMD", 2000)]
    [InlineData("0.29", "USD", 29)]
    public void TwoDecimalCurrenciesConvertToHundredths(string amount, string code, long expected) =>
        CurrencyCode.ToMinorUnits(decimal.Parse(amount, CultureInfo.InvariantCulture), code).Should().Be(expected);

    /// <summary>Zero-decimal currencies keep the major unit, rounding a fraction instead of dropping it.</summary>
    /// <param name="amount">The major-unit amount, invariant text so it reaches the test as an exact decimal.</param>
    /// <param name="code">The currency.</param>
    /// <param name="expected">The minor-unit amount.</param>
    [Theory]
    [InlineData("1500", "JPY", 1500)]
    [InlineData("1500.5", "jpy", 1501)]
    [InlineData("1500.4", "KRW", 1500)]
    public void ZeroDecimalCurrenciesKeepTheMajorUnit(string amount, string code, long expected) =>
        CurrencyCode.ToMinorUnits(decimal.Parse(amount, CultureInfo.InvariantCulture), code).Should().Be(expected);

    /// <summary>Three-decimal currencies convert to thousandths.</summary>
    /// <param name="amount">The major-unit amount, invariant text so it reaches the test as an exact decimal.</param>
    /// <param name="code">The currency.</param>
    /// <param name="expected">The minor-unit amount.</param>
    [Theory]
    [InlineData("1", "KWD", 1000)]
    [InlineData("1.0005", "kwd", 1001)]
    [InlineData("2.3334", "BHD", 2333)]
    public void ThreeDecimalCurrenciesConvertToThousandths(string amount, string code, long expected) =>
        CurrencyCode.ToMinorUnits(decimal.Parse(amount, CultureInfo.InvariantCulture), code).Should().Be(expected);

    /// <summary>A minor-unit amount converts back to major units by the currency's ISO 4217 exponent.</summary>
    /// <param name="amountMinor">The minor-unit amount.</param>
    /// <param name="code">The currency.</param>
    /// <param name="expected">The major-unit amount, invariant text so it reaches the test as an exact decimal.</param>
    [Theory]
    [InlineData(1001, "USD", "10.01")]
    [InlineData(1501, "jpy", "1501")]
    [InlineData(1001, "KWD", "1.001")]
    public void FromMinorUnitsConvertsBackToMajorUnits(long amountMinor, string code, string expected) =>
        CurrencyCode.FromMinorUnits(amountMinor, code).Should().Be(decimal.Parse(expected, CultureInfo.InvariantCulture));
}

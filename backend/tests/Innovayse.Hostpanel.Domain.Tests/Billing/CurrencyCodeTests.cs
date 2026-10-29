using System.Globalization;
using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Domain.Tests.Billing;

/// <summary>Unit tests for the <see cref="CurrencyCode"/> rule shared by invoices, orders, quotes and transactions.</summary>
public sealed class CurrencyCodeTests
{
    /// <summary>A three-letter code is accepted and stored in upper case.</summary>
    [Fact]
    public void Normalise_UpperCasesAThreeLetterCode()
    {
        CurrencyCode.Normalise("eur").Should().Be("EUR");
    }

    /// <summary>Anything but a three-letter code is refused.</summary>
    /// <param name="currency">A missing or malformed code.</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Normalise_RefusesAnythingButThreeLetters(string currency)
    {
        var act = () => CurrencyCode.Normalise(currency);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>The exponent is the ISO 4217 one, whatever the code's case.</summary>
    /// <param name="code">The currency.</param>
    /// <param name="expected">The exponent.</param>
    [Theory]
    [InlineData("USD", 2)]
    [InlineData("jpy", 0)]
    [InlineData("OMR", 3)]
    public void MinorUnitExponent_FollowsIso4217(string code, int expected)
    {
        CurrencyCode.MinorUnitExponent(code).Should().Be(expected);
    }

    /// <summary>An amount is rounded to the currency's minor unit, half away from zero.</summary>
    /// <param name="amount">The amount, invariant text so it reaches the test as an exact decimal.</param>
    /// <param name="code">The currency.</param>
    /// <param name="expected">The rounded amount, invariant text.</param>
    [Theory]
    [InlineData("10.005", "USD", "10.01")]
    [InlineData("33.3333", "EUR", "33.33")]
    [InlineData("1500.5", "JPY", "1501")]
    [InlineData("1.0005", "KWD", "1.001")]
    public void RoundToMinorUnits_RoundsToTheCurrencyMinorUnit(string amount, string code, string expected)
    {
        CurrencyCode.RoundToMinorUnits(decimal.Parse(amount, CultureInfo.InvariantCulture), code)
            .Should().Be(decimal.Parse(expected, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// An amount becomes whole minor units in a zero, two or three decimal currency, rounding half
    /// away from zero and never truncating, including amounts that do not divide evenly.
    /// </summary>
    /// <param name="amount">The amount, invariant text so it reaches the test as an exact decimal.</param>
    /// <param name="code">The currency.</param>
    /// <param name="expected">The minor-unit integer.</param>
    [Theory]
    [InlineData("10.00", "USD", 1000)]
    [InlineData("10.005", "USD", 1001)]
    [InlineData("0.999", "USD", 100)]
    [InlineData("19.994", "EUR", 1999)]
    [InlineData("1500", "JPY", 1500)]
    [InlineData("1500.5", "JPY", 1501)]
    [InlineData("1.0005", "KWD", 1001)]
    [InlineData("0.0004", "KWD", 0)]
    public void ToMinorUnits_RoundsAndNeverTruncates(string amount, string code, long expected)
    {
        CurrencyCode.ToMinorUnits(decimal.Parse(amount, CultureInfo.InvariantCulture), code).Should().Be(expected);
    }

    /// <summary>Minor units convert back to the exact major-unit amount in zero, two and three decimal currencies.</summary>
    /// <param name="minor">The minor-unit integer.</param>
    /// <param name="code">The currency.</param>
    /// <param name="expected">The major-unit amount, invariant text.</param>
    [Theory]
    [InlineData(1001, "USD", "10.01")]
    [InlineData(1501, "JPY", "1501")]
    [InlineData(1001, "KWD", "1.001")]
    public void FromMinorUnits_RestoresTheMajorUnitAmount(long minor, string code, string expected)
    {
        CurrencyCode.FromMinorUnits(minor, code).Should().Be(decimal.Parse(expected, CultureInfo.InvariantCulture));
    }
}

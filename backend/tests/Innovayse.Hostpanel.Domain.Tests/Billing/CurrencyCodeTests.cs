using FluentAssertions;
using Innovayse.Hostpanel.Domain.Billing;
using Xunit;

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
}

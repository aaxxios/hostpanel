using Innovayse.Hostpanel.Domain.Orders.Enums;
using Innovayse.Hostpanel.Infrastructure.Persistence.Converters;

namespace Innovayse.Hostpanel.Integration.Tests.Persistence;

/// <summary><see cref="DomainActionConverter"/> keeps the exact texts the <c>OrderItems.DomainAction</c> column has always held.</summary>
public sealed class DomainActionConverterTests
{
    /// <summary>Each action maps to its lower-case text and back.</summary>
    /// <param name="action">The action.</param>
    /// <param name="stored">The column value.</param>
    [Theory]
    [InlineData(DomainAction.Register, "register")]
    [InlineData(DomainAction.Transfer, "transfer")]
    [InlineData(DomainAction.Renew, "renew")]
    public void RoundTrip_UsesTheExactStoredText(DomainAction action, string stored)
    {
        DomainActionConverter.ToStored(action).Should().Be(stored);
        DomainActionConverter.FromStored(stored).Should().Be(action);
    }

    /// <summary>A text that names no action fails the read.</summary>
    /// <param name="stored">The column value.</param>
    [Theory]
    [InlineData("Register")]
    [InlineData("release")]
    [InlineData("")]
    public void FromStored_UnknownText_Throws(string stored)
    {
        var act = () => DomainActionConverter.FromStored(stored);

        act.Should().Throw<InvalidOperationException>();
    }
}

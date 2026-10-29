using Innovayse.Hostpanel.Infrastructure.Security;
using Innovayse.Hostpanel.Infrastructure.Security.Enums;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Security;

/// <summary>
/// <see cref="PublicHostGuard"/>: only a plain multi-label host name is ever resolved, and a name is
/// refused before any lookup when it is an IP literal, a single label or malformed.
/// </summary>
public sealed class PublicHostGuardTests
{
    /// <summary>Ordinary public host names pass the shape check.</summary>
    /// <param name="host">The host name under test.</param>
    [Theory]
    [InlineData("example.com")]
    [InlineData("www.example.co.uk")]
    [InlineData("my-site.example.org")]
    [InlineData("xn--bcher-kva.example")]
    public void IsPublicHostName_AnOrdinaryName_IsAccepted(string host)
    {
        PublicHostGuard.IsPublicHostName(host).Should().BeTrue();
    }

    /// <summary>IP literals in every spelling, single labels and malformed text fail the shape check.</summary>
    /// <param name="host">The text under test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("localhost")]
    [InlineData("intranet")]
    [InlineData("127.0.0.1")]
    [InlineData("127.1")]
    [InlineData("2130706433")]
    [InlineData("0x7f.0.0.1")]
    [InlineData("::1")]
    [InlineData("[::1]")]
    [InlineData("::ffff:10.0.0.1")]
    [InlineData("example.com.")]
    [InlineData(".example.com")]
    [InlineData("a..example.com")]
    [InlineData("-bad.example.com")]
    [InlineData("bad-.example.com")]
    [InlineData("exa_mple.com")]
    [InlineData("example.com:443")]
    [InlineData("example.com/path")]
    [InlineData("user@example.com")]
    public void IsPublicHostName_NotAPublicHostName_IsRefused(string? host)
    {
        PublicHostGuard.IsPublicHostName(host).Should().BeFalse();
    }

    /// <summary>A name longer than DNS allows is refused.</summary>
    [Fact]
    public void IsPublicHostName_ANameLongerThanDnsAllows_IsRefused()
    {
        var host = string.Join('.', Enumerable.Repeat(new string('a', 60), 5)) + ".com";

        PublicHostGuard.IsPublicHostName(host).Should().BeFalse();
    }

    /// <summary>A refused shape is answered without a lookup and carries no address.</summary>
    /// <param name="host">The text under test.</param>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Theory]
    [InlineData("169.254.169.254")]
    [InlineData("localhost")]
    [InlineData("metadata")]
    public async Task ResolveAsync_ARefusedShape_ReturnsRefusedWithNoAddresses(string host)
    {
        var resolution = await PublicHostGuard.ResolveAsync(host, CancellationToken.None);

        resolution.Verdict.Should().Be(PublicHostVerdict.Refused);
        resolution.Addresses.Should().BeEmpty();
    }
}

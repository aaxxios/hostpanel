using Innovayse.Hostpanel.Application.Services.Common;
using Innovayse.Hostpanel.Infrastructure.Monitoring;
using Innovayse.Hostpanel.Infrastructure.Time;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Monitoring;

/// <summary>
/// <see cref="HostingProbe"/>: a client-supplied domain that is not a public host is refused and
/// answered with the same empty reading as a domain with nothing to read, without any lookup.
/// </summary>
public sealed class HostingProbeTests
{
    /// <summary>The probe under test, on the real clock.</summary>
    private readonly HostingProbe _probe = new(new SystemClock(TimeProvider.System));

    /// <summary>IP literals, single-label names, malformed names and short IPv4 spellings are refused outright.</summary>
    /// <param name="domain">The client-supplied domain.</param>
    /// <returns>A task that completes when the assertion has run.</returns>
    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("169.254.169.254")]
    [InlineData("10.0.0.5")]
    [InlineData("[::1]")]
    [InlineData("::1")]
    [InlineData("localhost")]
    [InlineData("intranet")]
    [InlineData("db.local.")]
    [InlineData("127.1")]
    [InlineData("0x7f.0.0.1")]
    [InlineData("exa mple.com")]
    [InlineData("example.com/path")]
    [InlineData("user@example.com")]
    [InlineData("")]
    public async Task ProbeAsync_ANameThatIsNotAPublicHost_ReturnsTheEmptyReading(string domain)
    {
        var result = await _probe.ProbeAsync(new HostingProbeInput(domain), CancellationToken.None);

        result.Should().BeSameAs(HostingInfoDto.Empty);
    }

    /// <summary>A name under <c>.localhost</c> never yields a certificate or nameservers, whether it is refused after resolution or does not resolve at all.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task ProbeAsync_ALoopbackName_ReadsNothing()
    {
        var result = await _probe.ProbeAsync(new HostingProbeInput("service.localhost"), CancellationToken.None);

        result.Ssl.Should().BeNull();
        result.Nameservers.Should().BeEmpty();
    }
}

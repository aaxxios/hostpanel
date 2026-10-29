using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Common;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>Answers every domain with fixed nameservers and a valid certificate without opening a connection.</summary>
public sealed class DemoHostingProbe : IHostingProbe
{
    /// <summary>The issuer the fake certificate names.</summary>
    private const string Issuer = "Demo Certificate Authority";

    /// <summary>How long the fake certificate has been valid and will stay valid, on each side of now.</summary>
    private static readonly TimeSpan ValidityHalfWindow = TimeSpan.FromDays(180);

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Creates the probe.</summary>
    /// <param name="clock">The source of the current moment.</param>
    public DemoHostingProbe(IClock clock)
    {
        _clock = clock;
    }

    /// <inheritdoc/>
    public Task<HostingInfoDto> ProbeAsync(HostingProbeInput input, CancellationToken ct)
    {
        var now = _clock.UtcNow;

        return Task.FromResult(new HostingInfoDto(
            ["ns1.demo.example", "ns2.demo.example"],
            new HostingSslDto(true, Issuer, now - ValidityHalfWindow, now + ValidityHalfWindow)));
    }
}

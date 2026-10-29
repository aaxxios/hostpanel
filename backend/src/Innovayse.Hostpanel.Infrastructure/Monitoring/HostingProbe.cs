using DnsClient;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Common;
using Innovayse.Hostpanel.Infrastructure.Security;
using Innovayse.Hostpanel.Infrastructure.Security.Enums;

namespace Innovayse.Hostpanel.Infrastructure.Monitoring;

/// <summary>
/// Reads a client's domain over the network: its nameservers through DNS and its certificate through
/// a TLS handshake on port 443.
/// </summary>
/// <remarks>
/// The domain is client-supplied, so <see cref="PublicHostGuard"/> decides first and nothing is
/// looked up or dialled for a refused name. The certificate is read from the addresses the guard
/// vetted, never from the name again. Every step has its own timeout, and a refusal is the same empty
/// answer as a domain with nothing to read.
/// </remarks>
public sealed class HostingProbe : IHostingProbe
{
    /// <summary>How long the nameserver lookup may take.</summary>
    private static readonly TimeSpan LookupTimeout = TimeSpan.FromSeconds(3);

    /// <summary>The resolver client; thread safe, so one serves every probe.</summary>
    private static readonly LookupClient Resolver = new(new LookupClientOptions
    {
        Timeout = LookupTimeout,
        Retries = 1,
        UseCache = false,
    });

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the probe.</summary>
    /// <param name="clock">The source of the current moment, which decides whether a certificate is inside its validity window.</param>
    public HostingProbe(IClock clock)
    {
        _clock = clock;
    }

    /// <inheritdoc/>
    public async Task<HostingInfoDto> ProbeAsync(HostingProbeInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);

        var resolution = await PublicHostGuard.ResolveAsync(input.Domain, ct);
        if (resolution.Verdict == PublicHostVerdict.Refused)
        {
            return HostingInfoDto.Empty;
        }

        // Independent network calls, so they run side by side.
        var nameservers = ReadNameserversAsync(input.Domain, ct);
        var certificate = resolution.Verdict == PublicHostVerdict.Public
            ? TlsCertificateReader.ReadAsync(input.Domain, resolution.Addresses, ct)
            : Task.FromResult<TlsCertificate?>(null);
        await Task.WhenAll(nameservers, certificate);

        var cert = await certificate;
        var ssl = cert is null
            ? null
            : new HostingSslDto(IsValid(cert), cert.Issuer, cert.NotBefore, cert.NotAfter);

        return new HostingInfoDto(await nameservers, ssl);
    }

    /// <summary>Whether the platform trusted the certificate and the present moment lies inside its validity window.</summary>
    /// <param name="cert">The certificate that was read.</param>
    /// <returns><see langword="true"/> for a trusted certificate that is neither expired nor not yet valid.</returns>
    private bool IsValid(TlsCertificate cert)
    {
        var now = _clock.UtcNow;
        return cert.IsTrusted && now >= cert.NotBefore && now <= cert.NotAfter;
    }

    /// <summary>Looks up the nameservers of a domain.</summary>
    /// <param name="domain">A domain the guard accepted.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The nameserver host names, sorted; empty when the lookup fails or finds none.</returns>
    private static async Task<IReadOnlyList<string>> ReadNameserversAsync(string domain, CancellationToken ct)
    {
        try
        {
            var response = await Resolver.QueryAsync(domain, QueryType.NS, cancellationToken: ct);
            return response.Answers
                .NsRecords()
                .Select(record => record.NSDName.Value.TrimEnd('.'))
                .Order(StringComparer.Ordinal)
                .ToList();
        }
        catch (Exception exception) when (exception is DnsResponseException or TimeoutException)
        {
            // An unanswered or refused lookup means "nothing to show", not a fault of the request.
            return [];
        }
    }
}

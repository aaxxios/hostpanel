namespace Innovayse.Hostpanel.Infrastructure.Monitoring;

/// <summary>The TLS probe result of one domain, with the domain facts the stored check records.</summary>
/// <param name="Name">The domain name.</param>
/// <param name="IsActive">Whether the domain is active.</param>
/// <param name="Probe">What the probe found.</param>
internal sealed record DomainSslProbe(string Name, bool IsActive, SslProbeResult Probe);

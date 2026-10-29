namespace Innovayse.Hostpanel.Infrastructure.Monitoring;

/// <summary>What a TLS probe of one host found.</summary>
/// <param name="HasSsl">Whether the host presented a certificate.</param>
/// <param name="Issuer">The certificate issuer, or <see langword="null"/> when none was presented.</param>
/// <param name="ExpiresAt">The certificate expiry, or <see langword="null"/> when none was presented.</param>
internal sealed record SslProbeResult(bool HasSsl, string? Issuer, DateTimeOffset? ExpiresAt);

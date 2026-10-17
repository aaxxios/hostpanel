namespace Innovayse.Hostpanel.Shared.Settings;

/// <summary>
/// The reverse proxies whose <c>X-Forwarded-For</c> / <c>X-Forwarded-Proto</c> headers are trusted.
/// Bound from the "ForwardedHeaders" section. No defaults: an empty list trusts nobody, so the
/// client IP stays the TCP peer and a caller cannot pick its own rate-limit partition with a
/// forged header.
/// </summary>
public sealed class ForwardedHeadersOptions
{
    /// <summary>The configuration section these settings bind from.</summary>
    public const string SectionName = "ForwardedHeaders";

    /// <summary>
    /// Single proxy addresses, IPv4 or IPv6, e.g. <c>172.18.0.5</c>. Array; env form
    /// <c>ForwardedHeaders__KnownProxies__0</c>.
    /// </summary>
    public string[] KnownProxies { get; init; } = [];

    /// <summary>
    /// Proxy networks in CIDR form, e.g. <c>172.18.0.0/16</c>. Array; env form
    /// <c>ForwardedHeaders__KnownNetworks__0</c>.
    /// </summary>
    public string[] KnownNetworks { get; init; } = [];
}

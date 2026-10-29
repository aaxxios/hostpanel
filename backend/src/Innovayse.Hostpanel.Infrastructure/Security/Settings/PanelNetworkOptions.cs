using System.Net;

namespace Innovayse.Hostpanel.Infrastructure.Security.Settings;

/// <summary>
/// Private routes to control panels, bound from the <c>PanelNetwork</c> section.
/// </summary>
/// <remarks>
/// A panel's API port is often closed to the internet and reachable only over a VPN. Pointing the
/// server's hostname at the VPN address is not an option: the same hostname builds the links
/// customers open, and the panel's certificate names the hostname, not the VPN address. An entry
/// here makes HostPanel dial the VPN address for that hostname while every request, and so TLS
/// validation, keeps the hostname.
/// </remarks>
public sealed class PanelNetworkOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "PanelNetwork";

    /// <summary>
    /// Comma-separated <c>hostname=address</c> pairs, for example
    /// <c>srv4.example.com=10.77.0.1</c>. Empty when not set: every panel is dialled at the
    /// address its hostname resolves to.
    /// </summary>
    public string? HostAddresses { get; set; }

    /// <summary>Gets whether every pair names a host and a plain IPv4 or IPv6 address, once.</summary>
    public bool IsValid => TryParse(out _);

    /// <summary>Gets the configured hostname-to-address map; empty when the value is invalid.</summary>
    public IReadOnlyDictionary<string, IPAddress> Map =>
        TryParse(out var map) ? map : new Dictionary<string, IPAddress>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Parses <see cref="HostAddresses"/>.</summary>
    /// <param name="map">The parsed map, keyed case-insensitively by hostname.</param>
    /// <returns><see langword="true"/> when every pair is well formed and no host repeats.</returns>
    private bool TryParse(out Dictionary<string, IPAddress> map)
    {
        map = new Dictionary<string, IPAddress>(StringComparer.OrdinalIgnoreCase);
        var pairs = (HostAddresses ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || parts[0].Length == 0 || !IPAddress.TryParse(parts[1], out var address)
                || !map.TryAdd(parts[0], address))
            {
                return false;
            }
        }

        return true;
    }
}

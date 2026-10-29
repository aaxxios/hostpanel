using System.ComponentModel.DataAnnotations;

namespace Innovayse.Hostpanel.Infrastructure.Security.Settings;

/// <summary>
/// Which addresses the clients that dial admin-entered hosts may reach (a provisioning server, a
/// registrar API, an SSH key agent), and how long a connection attempt may take. Bound from the
/// <c>OutboundNetwork</c> section.
/// </summary>
/// <remarks>
/// Loopback, link-local and cloud metadata addresses are refused whatever these options say: an
/// admin-entered host must never become a way to reach this server's own services or the
/// metadata endpoint. Private ranges are allowed by default because hosting panels and their
/// servers usually sit on the same private network; a deployment whose servers are all public
/// can switch them off.
/// </remarks>
public sealed class OutboundNetworkOptions
{
    /// <summary>The configuration section these options bind from.</summary>
    public const string SectionName = "OutboundNetwork";

    /// <summary>
    /// Whether a host resolving to a private address (10/8, 172.16/12, 192.168/16, 100.64/10,
    /// fc00::/7) may be dialled. On by default, since servers usually live on a private network.
    /// </summary>
    public bool AllowPrivateNetworks { get; init; } = true;

    /// <summary>How long name resolution plus the TCP connect may take, in seconds.</summary>
    [Range(1, 120)]
    public int ConnectTimeoutSeconds { get; init; } = 10;

    /// <summary>Gets <see cref="ConnectTimeoutSeconds"/> as a <see cref="TimeSpan"/>.</summary>
    public TimeSpan ConnectTimeout => TimeSpan.FromSeconds(ConnectTimeoutSeconds);
}

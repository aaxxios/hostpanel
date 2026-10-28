using System.Net;

namespace Innovayse.Hostpanel.Application.Common.Settings;

/// <summary>
/// How managed servers see this HostPanel, for the ready-made firewall and sshd lines the admin
/// panel shows when a server is connected.
/// </summary>
public sealed class SshAgentOptions
{
    /// <summary>The configuration section.</summary>
    public const string SectionName = "SshAgent";

    /// <summary>
    /// The address or addresses HostPanel's connections to agents come from, as the managed server
    /// sees them (after any NAT), comma-separated; for example <c>203.0.113.10</c>. Empty when not
    /// set, and the admin panel then asks for it instead of guessing.
    /// </summary>
    public string? SourceAddresses { get; set; }

    /// <summary>Gets the configured addresses, trimmed, in order.</summary>
    public IReadOnlyList<string> Addresses =>
        (SourceAddresses ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>Gets whether every configured address is a plain IPv4 or IPv6 address.</summary>
    public bool IsValid => Addresses.All(a => IPAddress.TryParse(a, out _));
}

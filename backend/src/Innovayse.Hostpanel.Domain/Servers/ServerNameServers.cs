namespace Innovayse.Hostpanel.Domain.Servers;

/// <summary>The up to five nameservers (hostname and IP each) a server's accounts are delegated to.</summary>
/// <param name="Ns1Hostname">Primary nameserver hostname.</param>
/// <param name="Ns1Ip">Primary nameserver IP.</param>
/// <param name="Ns2Hostname">Secondary nameserver hostname.</param>
/// <param name="Ns2Ip">Secondary nameserver IP.</param>
/// <param name="Ns3Hostname">Third nameserver hostname.</param>
/// <param name="Ns3Ip">Third nameserver IP.</param>
/// <param name="Ns4Hostname">Fourth nameserver hostname.</param>
/// <param name="Ns4Ip">Fourth nameserver IP.</param>
/// <param name="Ns5Hostname">Fifth nameserver hostname.</param>
/// <param name="Ns5Ip">Fifth nameserver IP.</param>
public sealed record ServerNameServers(
    string? Ns1Hostname,
    string? Ns1Ip,
    string? Ns2Hostname,
    string? Ns2Ip,
    string? Ns3Hostname,
    string? Ns3Ip,
    string? Ns4Hostname,
    string? Ns4Ip,
    string? Ns5Hostname,
    string? Ns5Ip)
{
    /// <summary>Gets the value that names no nameserver.</summary>
    public static ServerNameServers None { get; } = new(null, null, null, null, null, null, null, null, null, null);
}

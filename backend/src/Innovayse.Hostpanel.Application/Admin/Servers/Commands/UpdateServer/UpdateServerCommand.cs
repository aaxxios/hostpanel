using Innovayse.Hostpanel.Domain.Servers;

using Innovayse.Hostpanel.Domain.Servers.Enums;
namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.UpdateServer;

/// <summary>
/// Updates an existing provisioning server's full configuration.
/// </summary>
/// <remarks>
/// Carries every field flat, the same shape as <see cref="Commands.CreateServer.CreateServerCommand"/>,
/// so the command binds directly against the admin panel's existing flat request body.
/// </remarks>
/// <param name="Id">
/// Server primary key; wins over any id in the body. A value type, so it is left non-nullable — a
/// missing <c>int</c> in the body binds to <c>0</c> rather than being treated as a required
/// member, and the route overlay (<c>command with { Id = id }</c>) replaces it.
/// </param>
/// <param name="Name">Display name for the server.</param>
/// <param name="Hostname">Hostname or IP address used for API access.</param>
/// <param name="IpAddress">Primary public IP address, or null.</param>
/// <param name="AssignedIpAddresses">Additional IPs (newline-separated), or null.</param>
/// <param name="Module">Control panel module type.</param>
/// <param name="Username">API username.</param>
/// <param name="Password">API password, or null to leave the stored one unchanged.</param>
/// <param name="ApiToken">API token, or null to leave the stored one unchanged.</param>
/// <param name="AccessHash">Access hash for hosting panel authentication, or null to leave unchanged.</param>
/// <param name="UseSSL">Whether SSL should be used for API connections.</param>
/// <param name="MaxAccounts">Account capacity limit, or null for unlimited.</param>
/// <param name="IsDefault">Whether this is the default server for its module type.</param>
/// <param name="IsDisabled">Whether new accounts should be blocked from this server.</param>
/// <param name="MonthlyCost">Monthly cost in USD.</param>
/// <param name="Datacenter">Datacenter or NOC provider name, or null.</param>
/// <param name="ServerStatusAddress">URL to the server status page folder, or null.</param>
/// <param name="Ns1Hostname">Primary nameserver hostname, or null.</param>
/// <param name="Ns1Ip">Primary nameserver IP, or null.</param>
/// <param name="Ns2Hostname">Secondary nameserver hostname, or null.</param>
/// <param name="Ns2Ip">Secondary nameserver IP, or null.</param>
/// <param name="Ns3Hostname">Third nameserver hostname, or null.</param>
/// <param name="Ns3Ip">Third nameserver IP, or null.</param>
/// <param name="Ns4Hostname">Fourth nameserver hostname, or null.</param>
/// <param name="Ns4Ip">Fourth nameserver IP, or null.</param>
/// <param name="Ns5Hostname">Fifth nameserver hostname, or null.</param>
/// <param name="Ns5Ip">Fifth nameserver IP, or null.</param>
public sealed record UpdateServerCommand(
    int Id,
    string Name,
    string Hostname,
    string? IpAddress,
    string? AssignedIpAddresses,
    ServerModule Module,
    string Username,
    string? Password,
    string? ApiToken,
    string? AccessHash,
    bool UseSSL,
    int? MaxAccounts,
    bool IsDefault,
    bool IsDisabled,
    decimal MonthlyCost,
    string? Datacenter,
    string? ServerStatusAddress,
    string? Ns1Hostname,
    string? Ns1Ip,
    string? Ns2Hostname,
    string? Ns2Ip,
    string? Ns3Hostname,
    string? Ns3Ip,
    string? Ns4Hostname,
    string? Ns4Ip,
    string? Ns5Hostname,
    string? Ns5Ip);

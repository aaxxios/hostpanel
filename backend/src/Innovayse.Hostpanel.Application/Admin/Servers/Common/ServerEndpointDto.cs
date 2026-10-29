namespace Innovayse.Hostpanel.Application.Admin.Servers.Common;

/// <summary>Where and how a provisioning server's API is reached.</summary>
/// <param name="Hostname">Hostname or IP address used for API access.</param>
/// <param name="IpAddress">Primary public IP address, or null.</param>
/// <param name="AssignedIpAddresses">Additional IPs (newline-separated), or null.</param>
/// <param name="UseSSL">Whether SSL is used for API connections.</param>
public sealed record ServerEndpointDto(string Hostname, string? IpAddress, string? AssignedIpAddresses, bool UseSSL);

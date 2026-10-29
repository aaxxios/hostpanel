namespace Innovayse.Hostpanel.Domain.Provisioning;

/// <summary>
/// Hosting service credentials returned to the client after provisioning.
/// </summary>
/// <param name="Username">Control-panel login username.</param>
/// <param name="Password">Control-panel login password, or null when the panel never reveals a stored password.</param>
/// <param name="Domain">Primary domain name associated with the account.</param>
/// <param name="ServerIp">IPv4 address of the hosting server.</param>
/// <param name="CpanelUrl">Full URL to the control panel login page for this account.</param>
public record ServiceCredentials(string Username, string? Password, string Domain, string ServerIp, string CpanelUrl);

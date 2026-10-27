namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.RevokeServerSshKeys;

/// <summary>
/// Admin: revokes every HostPanel-managed SSH key issued on a server — before uninstalling its
/// agent or decommissioning it. Keys added by other means are never touched.
/// </summary>
/// <param name="ServerId">The server.</param>
public sealed record RevokeServerSshKeysCommand(int ServerId);

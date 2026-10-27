namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.ScanServerSshHostKey;

/// <summary>Admin: reads the host key the server's agent endpoint presents, without trusting it.</summary>
/// <param name="ServerId">The server.</param>
public sealed record ScanServerSshHostKeyCommand(int ServerId);

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.GenerateServerSshIdentity;

/// <summary>Admin: generates (or rotates) a server's ED25519 management identity.</summary>
/// <param name="ServerId">The server.</param>
public sealed record GenerateServerSshIdentityCommand(int ServerId);

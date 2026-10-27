namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.TestServerSshAgent;

/// <summary>Admin: pings a server's SSH key agent and records the outcome.</summary>
/// <param name="ServerId">The server.</param>
public sealed record TestServerSshAgentCommand(int ServerId);

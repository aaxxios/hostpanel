namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.TestServerConnection;

/// <summary>Command to test connectivity to a specific server and persist the result.</summary>
/// <param name="ServerId">The server to test.</param>
public sealed record TestServerConnectionCommand(int ServerId);

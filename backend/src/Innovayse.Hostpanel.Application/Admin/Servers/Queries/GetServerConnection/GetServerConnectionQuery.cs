namespace Innovayse.Hostpanel.Application.Admin.Servers.Queries.GetServerConnection;

/// <summary>Internal: reads what a provisioning provider needs to reach one server. Dispatched over the bus, never routed.</summary>
/// <param name="ServerId">The server.</param>
public sealed record GetServerConnectionQuery(int ServerId);

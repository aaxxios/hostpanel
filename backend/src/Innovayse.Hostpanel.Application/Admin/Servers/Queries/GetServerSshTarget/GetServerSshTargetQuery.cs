namespace Innovayse.Hostpanel.Application.Admin.Servers.Queries.GetServerSshTarget;

/// <summary>Internal: reads what an SSH key operation needs about one server. Dispatched over the bus, never routed.</summary>
/// <param name="ServerId">The server.</param>
public sealed record GetServerSshTargetQuery(int ServerId);

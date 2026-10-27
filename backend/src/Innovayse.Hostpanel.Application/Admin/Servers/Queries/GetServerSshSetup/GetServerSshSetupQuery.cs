namespace Innovayse.Hostpanel.Application.Admin.Servers.Queries.GetServerSshSetup;

/// <summary>Admin: reads what is needed to install the SSH key agent on a server.</summary>
/// <param name="ServerId">The server.</param>
public sealed record GetServerSshSetupQuery(int ServerId);

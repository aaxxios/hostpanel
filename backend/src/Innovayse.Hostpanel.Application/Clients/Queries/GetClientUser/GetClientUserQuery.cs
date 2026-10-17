namespace Innovayse.Hostpanel.Application.Clients.Queries.GetClientUser;

/// <summary>Query for the person linked to a client account, as the admin detail screen shows them.</summary>
/// <param name="ClientId">The client primary key.</param>
public sealed record GetClientUserQuery(int ClientId);

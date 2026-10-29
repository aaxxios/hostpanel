namespace Innovayse.Hostpanel.Application.Clients.Queries.GetClient;

/// <summary>Query to retrieve full details for a single client by primary key.</summary>
/// <param name="ClientId">The client's primary key.</param>
public sealed record GetClientQuery(int ClientId);

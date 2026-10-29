namespace Innovayse.Hostpanel.Application.Clients.Queries.GetClientSummary;

/// <summary>Query to retrieve aggregated summary data for a client profile dashboard.</summary>
/// <param name="ClientId">The client's primary key.</param>
public sealed record GetClientSummaryQuery(int ClientId);

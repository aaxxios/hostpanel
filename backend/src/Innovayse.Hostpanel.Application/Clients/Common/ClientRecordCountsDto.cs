namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>How many services, domains, quotes and tickets a client has.</summary>
/// <param name="ActiveServicesCount">Number of services with Active status.</param>
/// <param name="TotalServicesCount">Total number of services.</param>
/// <param name="TotalDomainsCount">Total number of domains.</param>
/// <param name="AcceptedQuotesCount">Number of quotes with Accepted stage.</param>
/// <param name="TotalQuotesCount">Total number of quotes.</param>
/// <param name="OpenTicketsCount">Number of open tickets.</param>
/// <param name="TotalTicketsCount">Total number of tickets.</param>
public sealed record ClientRecordCountsDto(
    int ActiveServicesCount,
    int TotalServicesCount,
    int TotalDomainsCount,
    int AcceptedQuotesCount,
    int TotalQuotesCount,
    int OpenTicketsCount,
    int TotalTicketsCount);

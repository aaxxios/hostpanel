namespace Innovayse.Hostpanel.Application.Support.Queries.ListClientTickets;

/// <summary>Query to list a client's tickets with pagination and optional subject search.</summary>
/// <param name="ClientId">FK to the client.</param>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Number of items per page.</param>
/// <param name="Search">Optional subject search term.</param>
public sealed record ListClientTicketsQuery(int ClientId, int Page, int PageSize, string? Search);

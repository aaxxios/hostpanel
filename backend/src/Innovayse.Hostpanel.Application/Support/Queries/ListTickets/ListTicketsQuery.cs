namespace Innovayse.Hostpanel.Application.Support.Queries.ListTickets;

/// <summary>Query to retrieve a paged, optionally filtered list of all support tickets.</summary>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Number of items per page.</param>
/// <param name="Status">Optional status filter (e.g. "Open", "Closed", "flagged").</param>
/// <param name="Search">Optional subject search term.</param>
public sealed record ListTicketsQuery(int Page = 1, int PageSize = 20, string? Status = null, string? Search = null);

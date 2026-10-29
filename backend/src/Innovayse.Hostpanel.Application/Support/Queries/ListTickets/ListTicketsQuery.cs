using Innovayse.Hostpanel.Domain.Support.Enums;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListTickets;

/// <summary>Query to retrieve a paged, optionally filtered list of all support tickets.</summary>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Number of items per page.</param>
/// <param name="Status">Optional status filter, bound by name from the query string; <see langword="null"/> for every status.</param>
/// <param name="Flagged">When <see langword="true"/>, only tickets staff flagged are listed.</param>
/// <param name="Search">Optional subject search term.</param>
public sealed record ListTicketsQuery(
    int Page = 1,
    int PageSize = 20,
    TicketStatus? Status = null,
    bool Flagged = false,
    string? Search = null);

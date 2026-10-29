using Innovayse.Hostpanel.Domain.Support.Enums;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListNetworkIssues;

/// <summary>Query to retrieve a paged, optionally filtered list of network issues.</summary>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Number of items per page.</param>
/// <param name="Statuses">
/// Optional status filter, bound by name from a repeated <c>statuses</c> query parameter so one list
/// tab can cover several statuses; <see langword="null"/> or empty for every status.
/// </param>
public sealed record ListNetworkIssuesQuery(int Page, int PageSize, NetworkIssueStatus[]? Statuses = null);

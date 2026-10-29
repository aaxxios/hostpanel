namespace Innovayse.Hostpanel.Application.Notifications.Queries.ListClientEmailLogs;

/// <summary>Query to retrieve a paginated list of email logs sent to a specific client.</summary>
/// <param name="ClientId">The client's primary key.</param>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Number of entries per page (clamped to 1 to 100).</param>
public sealed record ListClientEmailLogsQuery(int ClientId, int Page = 1, int PageSize = 20);

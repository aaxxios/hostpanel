namespace Innovayse.Hostpanel.Application.Notifications.Queries.ListMyEmailLogs;

/// <summary>Query for a page of the emails sent to the calling client. Names no account: the handler resolves the caller.</summary>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Number of entries per page (clamped to 1-100 by the underlying read).</param>
public sealed record ListMyEmailLogsQuery(int Page = 1, int PageSize = 20);

namespace Innovayse.Hostpanel.Application.Notifications.Queries.GetMyEmailLog;

/// <summary>Query for one email the calling client was sent. Names no account: the handler resolves the caller.</summary>
/// <param name="EmailLogId">The log entry's primary key.</param>
public sealed record GetMyEmailLogQuery(int EmailLogId);

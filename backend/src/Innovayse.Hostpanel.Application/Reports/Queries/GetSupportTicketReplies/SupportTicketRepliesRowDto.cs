namespace Innovayse.Hostpanel.Application.Reports.Queries.GetSupportTicketReplies;

/// <summary>One admin row in the Support Ticket Replies report.</summary>
/// <param name="AdminName">The admin name.</param>
/// <param name="DailyCounts">The daily counts entries.</param>
/// <param name="Total">The total.</param>
public record SupportTicketRepliesRowDto(
    string AdminName,
    IReadOnlyList<int> DailyCounts,
    int Total);

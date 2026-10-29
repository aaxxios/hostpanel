namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>Full Support Ticket Replies report result.</summary>
/// <param name="Month">The month.</param>
/// <param name="Year">The year.</param>
/// <param name="DaysInMonth">The days in month.</param>
/// <param name="Rows">The rows entries.</param>
public record SupportTicketRepliesDto(
    int Month,
    int Year,
    int DaysInMonth,
    IReadOnlyList<SupportTicketRepliesRowDto> Rows);

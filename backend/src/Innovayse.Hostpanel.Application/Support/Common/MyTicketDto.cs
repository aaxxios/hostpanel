using Innovayse.Hostpanel.Domain.Support.Enums;

namespace Innovayse.Hostpanel.Application.Support.Common;

/// <summary>
/// The client's own support ticket with its replies, as the client area reads it. The staff
/// assignment, the staff flag and the staff tags of <see cref="TicketDto"/> are not part of it,
/// so they cannot reach the client by any path.
/// </summary>
/// <param name="Id">Ticket primary key.</param>
/// <param name="Subject">The ticket subject line.</param>
/// <param name="Message">The opening message body.</param>
/// <param name="Status">Current lifecycle status.</param>
/// <param name="Priority">Priority level.</param>
/// <param name="DepartmentId">The department the ticket is filed under, if any.</param>
/// <param name="DepartmentName">Name of that department, if any.</param>
/// <param name="CreatedAt">UTC timestamp when the ticket was created.</param>
/// <param name="Replies">Replies on the ticket, oldest first.</param>
/// <param name="Rating">The client's rating of the support received, if given.</param>
/// <param name="FeedbackComment">The client's feedback comment, if given.</param>
/// <param name="FeedbackAt">When the feedback was left, if it was.</param>
public sealed record MyTicketDto(
    int Id,
    string Subject,
    string Message,
    TicketStatus Status,
    TicketPriority Priority,
    int? DepartmentId,
    string? DepartmentName,
    DateTimeOffset CreatedAt,
    IReadOnlyList<TicketReplyDto> Replies,
    int? Rating,
    string? FeedbackComment,
    DateTimeOffset? FeedbackAt);

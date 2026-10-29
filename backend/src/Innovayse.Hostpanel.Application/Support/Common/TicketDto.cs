using Innovayse.Hostpanel.Domain.Support.Enums;

namespace Innovayse.Hostpanel.Application.Support.Common;

/// <summary>DTO representing a full support ticket with its replies.</summary>
/// <param name="Id">Ticket primary key.</param>
/// <param name="ClientId">FK to the client who opened the ticket.</param>
/// <param name="Subject">The ticket subject line.</param>
/// <param name="Message">The initial message body.</param>
/// <param name="Status">Current lifecycle status.</param>
/// <param name="Priority">Priority level.</param>
/// <param name="Department">The department this ticket is filed under.</param>
/// <param name="Assignee">The staff member this ticket is assigned to.</param>
/// <param name="CreatedAt">UTC timestamp when the ticket was created.</param>
/// <param name="IsFlagged">Whether this ticket has been flagged by staff.</param>
/// <param name="Replies">All replies on this ticket.</param>
/// <param name="Tags">Tags attached to this ticket.</param>
/// <param name="Feedback">Client feedback on the ticket; its members are null until the client leaves feedback.</param>
public record TicketDto(
    int Id,
    int ClientId,
    string Subject,
    string Message,
    TicketStatus Status,
    TicketPriority Priority,
    TicketDepartmentDto Department,
    TicketAssigneeDto Assignee,
    DateTimeOffset CreatedAt,
    bool IsFlagged,
    IReadOnlyList<TicketReplyDto> Replies,
    IReadOnlyList<string> Tags,
    TicketFeedbackDto Feedback);

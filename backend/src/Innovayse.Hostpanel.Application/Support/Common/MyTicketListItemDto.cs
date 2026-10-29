using Innovayse.Hostpanel.Domain.Support.Enums;

namespace Innovayse.Hostpanel.Application.Support.Common;

/// <summary>
/// One row of the client's own ticket list. The staff flag and the owner fields of
/// <see cref="TicketListItemDto"/> are not part of it.
/// </summary>
/// <param name="Id">Ticket primary key.</param>
/// <param name="Subject">The ticket subject line.</param>
/// <param name="Status">Current lifecycle status.</param>
/// <param name="Priority">Priority level.</param>
/// <param name="CreatedAt">UTC timestamp when the ticket was created.</param>
/// <param name="ReplyCount">Total number of replies on this ticket.</param>
/// <param name="DepartmentName">Name of the department, if any.</param>
/// <param name="LastReplyAt">UTC timestamp of the most recent reply, if any.</param>
public sealed record MyTicketListItemDto(
    int Id,
    string Subject,
    TicketStatus Status,
    TicketPriority Priority,
    DateTimeOffset CreatedAt,
    int ReplyCount,
    string? DepartmentName,
    DateTimeOffset? LastReplyAt);

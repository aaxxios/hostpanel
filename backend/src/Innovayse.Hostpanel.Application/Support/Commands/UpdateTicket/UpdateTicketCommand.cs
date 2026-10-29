using Innovayse.Hostpanel.Domain.Support.Enums;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateTicket;

/// <summary>Command to update a ticket's metadata (status, priority, department, assignment).</summary>
/// <param name="TicketId">The ticket identifier.</param>
/// <param name="Status">New status (its name on the wire), or <see langword="null"/> to leave unchanged.</param>
/// <param name="Priority">New priority (its name on the wire), or <see langword="null"/> to leave unchanged.</param>
/// <param name="DepartmentId">New department FK, or <see langword="null"/> to leave unchanged.</param>
/// <param name="AssignedToStaffId">New staff assignment FK, or <see langword="null"/> to leave unchanged.</param>
public sealed record UpdateTicketCommand(
    int TicketId,
    TicketStatus? Status,
    TicketPriority? Priority,
    int? DepartmentId,
    int? AssignedToStaffId);

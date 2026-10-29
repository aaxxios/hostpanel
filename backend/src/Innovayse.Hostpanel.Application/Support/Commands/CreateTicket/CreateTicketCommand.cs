using Innovayse.Hostpanel.Domain.Support.Enums;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateTicket;

/// <summary>Command for a client to open a new support ticket on their own account.</summary>
/// <remarks>
/// Carries no client id. Which account is resolved inside the handler from the credential:
/// an id here would let a caller file a ticket against somebody else's account. The admin
/// route that legitimately opens a ticket for another client is a separate use case,
/// <c>AdminCreateTicketCommand</c>.
/// </remarks>
/// <param name="Subject">The ticket subject line.</param>
/// <param name="Message">The initial message body.</param>
/// <param name="DepartmentId">
/// FK to the target department. Nullable so a form that sent no department reaches the validator
/// and is refused on the <c>departmentId</c> field, instead of failing model binding with no field.
/// </param>
/// <param name="Priority">Priority level (Low, Medium, High on the wire).</param>
public sealed record CreateTicketCommand(string Subject, string Message, int? DepartmentId, TicketPriority Priority);

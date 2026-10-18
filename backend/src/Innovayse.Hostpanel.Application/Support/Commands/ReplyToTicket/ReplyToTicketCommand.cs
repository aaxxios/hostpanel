namespace Innovayse.Hostpanel.Application.Support.Commands.ReplyToTicket;

/// <summary>Command to post a reply on an existing support ticket; the author is the signed-in caller, resolved in the handler.</summary>
/// <param name="TicketId">The ID of the ticket to reply to.</param>
/// <param name="Message">The reply body text.</param>
/// <param name="IsStaffReply">Whether the author is a staff member.</param>
public sealed record ReplyToTicketCommand(int TicketId, string Message, bool IsStaffReply);

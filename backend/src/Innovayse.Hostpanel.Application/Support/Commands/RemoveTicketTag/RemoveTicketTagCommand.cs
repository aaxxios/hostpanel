namespace Innovayse.Hostpanel.Application.Support.Commands.RemoveTicketTag;

/// <summary>Removes a tag from a ticket.</summary>
/// <param name="TicketId">The ticket identifier.</param>
/// <param name="Tag">The tag text.</param>
public sealed record RemoveTicketTagCommand(int TicketId, string Tag);

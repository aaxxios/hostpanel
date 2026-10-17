namespace Innovayse.Hostpanel.Application.Support.Commands.AddTicketTag;

/// <summary>Adds a tag to a ticket.</summary>
/// <param name="TicketId">The ticket identifier.</param>
/// <param name="Tag">The tag text.</param>
public sealed record AddTicketTagCommand(int TicketId, string Tag);

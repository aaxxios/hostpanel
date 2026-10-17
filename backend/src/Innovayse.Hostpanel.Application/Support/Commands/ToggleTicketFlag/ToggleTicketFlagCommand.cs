namespace Innovayse.Hostpanel.Application.Support.Commands.ToggleTicketFlag;

/// <summary>Command to toggle the flagged state of a support ticket.</summary>
/// <param name="TicketId">The ticket identifier.</param>
public sealed record ToggleTicketFlagCommand(int TicketId);

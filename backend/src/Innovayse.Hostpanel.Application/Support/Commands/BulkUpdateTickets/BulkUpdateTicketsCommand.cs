namespace Innovayse.Hostpanel.Application.Support.Commands.BulkUpdateTickets;

/// <summary>Command to perform a bulk action on multiple tickets.</summary>
/// <param name="TicketIds">Array of ticket IDs to act upon.</param>
/// <param name="Action">Action to perform: <see cref="Enums.BulkTicketAction"/> name (any case).</param>
public sealed record BulkUpdateTicketsCommand(int[] TicketIds, string Action);

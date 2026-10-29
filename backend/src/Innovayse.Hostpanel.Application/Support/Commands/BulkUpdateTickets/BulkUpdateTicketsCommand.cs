using Innovayse.Hostpanel.Application.Support.Common.Enums;

namespace Innovayse.Hostpanel.Application.Support.Commands.BulkUpdateTickets;

/// <summary>Command to perform a bulk action on multiple tickets.</summary>
/// <param name="TicketIds">Array of ticket IDs to act upon.</param>
/// <param name="Action">Action to perform on every selected ticket.</param>
public sealed record BulkUpdateTicketsCommand(int[] TicketIds, BulkTicketAction Action);

namespace Innovayse.Hostpanel.Application.Migration.Common;

/// <summary>How many support and status records the legacy source holds.</summary>
/// <param name="Tickets">Support tickets available to pull.</param>
/// <param name="TicketReplies">Replies on support tickets available to pull.</param>
/// <param name="NetworkIssues">Network issues available to pull.</param>
public sealed record MigrationSupportTotals(int Tickets = 0, int TicketReplies = 0, int NetworkIssues = 0);

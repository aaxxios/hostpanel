using Innovayse.Hostpanel.Application.Billing.Common;

namespace Innovayse.Hostpanel.Application.Billing.Commands.CreateTimeBillingEntries;

/// <summary>Command to create multiple time billing entries as billable items for a client.</summary>
/// <param name="ClientId">FK to the client being charged; overlaid from the route.</param>
/// <param name="Entries">Time billing entries to create.</param>
public sealed record CreateTimeBillingEntriesCommand(int ClientId, IReadOnlyList<TimeBillingEntry> Entries);

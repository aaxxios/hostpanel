namespace Innovayse.Hostpanel.Application.Billing.Commands.InvoiceSelectedItems;

/// <summary>Command to create an invoice from selected uninvoiced billable items.</summary>
/// <param name="ClientId">FK to the client whose items are being invoiced; overlaid from the route.</param>
/// <param name="BillableItemIds">IDs of the billable items to invoice.</param>
public sealed record InvoiceSelectedItemsCommand(int ClientId, IReadOnlyList<int> BillableItemIds);

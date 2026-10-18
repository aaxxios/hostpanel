namespace Innovayse.Hostpanel.Application.Billing.Commands.UpdateInvoiceItems;

/// <summary>Command to add, update, or remove line items on an invoice.</summary>
/// <param name="InvoiceId">The invoice to modify.</param>
/// <param name="Items">The item entries describing changes.</param>
public sealed record UpdateInvoiceItemsCommand(int InvoiceId, IReadOnlyList<UpdateItemEntry> Items);

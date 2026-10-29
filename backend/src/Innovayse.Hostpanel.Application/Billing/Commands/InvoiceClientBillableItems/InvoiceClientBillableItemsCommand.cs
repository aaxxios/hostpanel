using Innovayse.Hostpanel.Domain.Billing.Enums;

namespace Innovayse.Hostpanel.Application.Billing.Commands.InvoiceClientBillableItems;

/// <summary>
/// Invoices every billable item of one type that is due for one client, on a single invoice. Sent by
/// the daily billable-items run, one message (and so one data context) per client, so a client whose
/// invoice cannot be written never stops or poisons the others.
/// </summary>
/// <param name="ClientId">The client whose due items are invoiced.</param>
/// <param name="Type">Whether the client's one-time items or recurring items are invoiced.</param>
public sealed record InvoiceClientBillableItemsCommand(int ClientId, BillableItemType Type);

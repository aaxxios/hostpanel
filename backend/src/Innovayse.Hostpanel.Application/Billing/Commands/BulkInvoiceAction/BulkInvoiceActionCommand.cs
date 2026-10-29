using Innovayse.Hostpanel.Application.Billing.Common.Enums;

namespace Innovayse.Hostpanel.Application.Billing.Commands.BulkInvoiceAction;

/// <summary>Command to perform a bulk action on multiple invoices.</summary>
/// <param name="InvoiceIds">The invoice IDs to act on.</param>
/// <param name="Action">The action to perform on each of them.</param>
public sealed record BulkInvoiceActionCommand(IReadOnlyList<int> InvoiceIds, InvoiceBulkAction Action);

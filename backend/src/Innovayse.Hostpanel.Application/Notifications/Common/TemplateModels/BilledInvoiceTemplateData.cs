namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>A new invoice as the invoice-created email reads it.</summary>
/// <param name="Id">The invoice id, read as <c>id</c>.</param>
/// <param name="Total">The invoice total, read as <c>total</c>.</param>
/// <param name="Currency">The ISO 4217 code the total is billed in, read as <c>currency</c>.</param>
public sealed record BilledInvoiceTemplateData(int Id, decimal Total, string Currency);

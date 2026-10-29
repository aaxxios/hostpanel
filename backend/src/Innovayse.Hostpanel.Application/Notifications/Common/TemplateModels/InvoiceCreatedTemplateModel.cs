namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the <c>invoice-created</c> email.</summary>
/// <param name="Invoice">The new invoice, read as <c>{{ invoice.id }}</c>, <c>{{ invoice.total }}</c> and <c>{{ invoice.currency }}</c>.</param>
public sealed record InvoiceCreatedTemplateModel(BilledInvoiceTemplateData Invoice) : EmailTemplateModel;

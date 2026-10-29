namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the <c>payment-received</c> email.</summary>
/// <param name="Invoice">The paid invoice and the payment, read as <c>{{ invoice.* }}</c>.</param>
public sealed record PaymentReceivedTemplateModel(PaidInvoiceTemplateData Invoice) : EmailTemplateModel;

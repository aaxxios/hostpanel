namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the renewal invoice reminder.</summary>
/// <param name="Invoice">The invoice that is due, read as <c>{{ invoice.* }}</c>.</param>
public sealed record RenewalReminderTemplateModel(RenewalInvoiceTemplateData Invoice) : EmailTemplateModel;

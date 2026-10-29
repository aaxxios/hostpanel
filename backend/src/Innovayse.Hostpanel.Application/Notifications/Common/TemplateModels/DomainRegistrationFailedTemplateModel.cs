namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the client notice about a failed domain registration.</summary>
/// <param name="Domain">The domain, read as <c>{{ domain.name }}</c>.</param>
/// <param name="Invoice">The invoice that paid for it, read as <c>{{ invoice.id }}</c>.</param>
public sealed record DomainRegistrationFailedTemplateModel(DomainNameTemplateData Domain, InvoiceRefTemplateData Invoice) : EmailTemplateModel;

namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the staff alert about a failed domain registration.</summary>
/// <param name="Domain">The domain, read as <c>{{ domain.name }}</c>.</param>
/// <param name="Client">The owning client, read as <c>{{ client.id }}</c>.</param>
/// <param name="Invoice">The invoice that paid for it, read as <c>{{ invoice.id }}</c>.</param>
/// <param name="Reason">Why the registration failed, read as <c>{{ reason }}</c>.</param>
public sealed record DomainRegistrationFailedAdminTemplateModel(DomainNameTemplateData Domain, ClientRefTemplateData Client, InvoiceRefTemplateData Invoice, string Reason) : EmailTemplateModel;

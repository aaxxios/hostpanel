namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the staff alert about a domain renewal that needs a person.</summary>
/// <param name="Domain">The domain, read as <c>{{ domain.name }}</c>.</param>
/// <param name="Client">The owning client, read as <c>{{ client.id }}</c>.</param>
/// <param name="Invoice">The renewal invoice, read as <c>{{ invoice.id }}</c>; its id is 0 when none was raised.</param>
/// <param name="Reason">Why the renewal could not complete, read as <c>{{ reason }}</c>.</param>
public sealed record DomainRenewalFailedAdminTemplateModel(DomainNameTemplateData Domain, ClientRefTemplateData Client, InvoiceRefTemplateData Invoice, string Reason) : EmailTemplateModel;

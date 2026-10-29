namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the <c>domain-renewed</c> email.</summary>
/// <param name="Domain">The renewed domain, read as <c>{{ domain.* }}</c>.</param>
public sealed record DomainRenewedTemplateModel(RenewedDomainTemplateData Domain) : EmailTemplateModel;

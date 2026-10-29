namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the <c>domain-expiring</c> email.</summary>
/// <param name="Domain">The expiring domain, read as <c>{{ domain.* }}</c>.</param>
public sealed record DomainExpiringTemplateModel(ExpiringDomainTemplateData Domain) : EmailTemplateModel;

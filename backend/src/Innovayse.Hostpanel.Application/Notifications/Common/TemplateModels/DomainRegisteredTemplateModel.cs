namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the <c>domain-registered</c> email.</summary>
/// <param name="Domain">The registered domain, read as <c>{{ domain.* }}</c>.</param>
public sealed record DomainRegisteredTemplateModel(RegisteredDomainTemplateData Domain) : EmailTemplateModel;

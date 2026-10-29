namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the <c>domain-expired</c> email.</summary>
/// <param name="Domain">The expired domain, read as <c>{{ domain.* }}</c>.</param>
public sealed record DomainExpiredTemplateModel(ExpiredDomainTemplateData Domain) : EmailTemplateModel;

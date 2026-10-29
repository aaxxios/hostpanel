namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the <c>domain-transferred</c> email.</summary>
/// <param name="Domain">The transferred domain, read as <c>{{ domain.* }}</c>.</param>
public sealed record DomainTransferredTemplateModel(RegisteredDomainTemplateData Domain) : EmailTemplateModel;

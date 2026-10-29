namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the <c>service-provisioned</c> email.</summary>
/// <param name="Service">The provisioned service, read as <c>{{ service.* }}</c>.</param>
public sealed record ServiceProvisionedTemplateModel(ProvisionedServiceTemplateData Service) : EmailTemplateModel;

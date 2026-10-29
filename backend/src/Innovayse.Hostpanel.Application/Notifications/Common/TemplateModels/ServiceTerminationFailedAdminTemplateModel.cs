namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>Data for the staff alert about an automatic termination that could not remove the hosting account.</summary>
/// <param name="Service">The service, read as <c>{{ service.id }}</c>.</param>
/// <param name="Reason">Why the termination failed, read as <c>{{ reason }}</c>.</param>
public sealed record ServiceTerminationFailedAdminTemplateModel(ServiceRefTemplateData Service, string Reason) : EmailTemplateModel;

namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>A provisioned service as its email reads it.</summary>
/// <param name="Id">The service id, read as <c>id</c>.</param>
/// <param name="ProvisioningRef">The panel's reference for the account, read as <c>provisioning_ref</c>.</param>
/// <param name="Domain">The service's domain, when it has one, read as <c>domain</c>.</param>
/// <param name="Username">The panel user name, when it has one, read as <c>username</c>.</param>
public sealed record ProvisionedServiceTemplateData(int Id, string ProvisioningRef, string? Domain, string? Username);

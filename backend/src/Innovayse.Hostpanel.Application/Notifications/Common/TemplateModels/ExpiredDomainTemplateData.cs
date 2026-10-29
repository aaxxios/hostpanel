namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>An expired domain as its email reads it.</summary>
/// <param name="Id">The domain id, read as <c>id</c>.</param>
/// <param name="Name">The domain name, when the row still exists, read as <c>name</c>.</param>
/// <param name="LinkedServiceId">The hosting service tied to it, if any, read as <c>linked_service_id</c>.</param>
public sealed record ExpiredDomainTemplateData(int Id, string? Name, int? LinkedServiceId);

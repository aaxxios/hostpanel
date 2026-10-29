namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>A renewed domain as its email reads it.</summary>
/// <param name="Id">The domain id, read as <c>id</c>.</param>
/// <param name="Name">The domain name, when the row still exists, read as <c>name</c>.</param>
/// <param name="NewExpiresAt">The new expiry date as <c>yyyy-MM-dd</c>, read as <c>new_expires_at</c>.</param>
public sealed record RenewedDomainTemplateData(int Id, string? Name, string NewExpiresAt);

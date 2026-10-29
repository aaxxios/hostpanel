namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>A registered or transferred domain as its email reads it.</summary>
/// <param name="Id">The domain id, read as <c>id</c>.</param>
/// <param name="Name">The domain name, read as <c>name</c>.</param>
/// <param name="Registrar">The registrar, when known, read as <c>registrar</c>.</param>
/// <param name="ExpiresAt">The expiry date as <c>yyyy-MM-dd</c>, when known, read as <c>expires_at</c>.</param>
public sealed record RegisteredDomainTemplateData(int Id, string Name, string? Registrar, string? ExpiresAt);

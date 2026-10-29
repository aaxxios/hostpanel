namespace Innovayse.Hostpanel.Application.Notifications.Common.TemplateModels;

/// <summary>A domain close to expiry as its email reads it.</summary>
/// <param name="Id">The domain id, read as <c>id</c>.</param>
/// <param name="Name">The domain name, read as <c>name</c>.</param>
/// <param name="ExpiresAt">The expiry date as <c>yyyy-MM-dd</c>, read as <c>expires_at</c>.</param>
/// <param name="DaysUntilExpiry">Whole days left, rounded up, read as <c>days_until_expiry</c>.</param>
public sealed record ExpiringDomainTemplateData(int Id, string Name, string ExpiresAt, int DaysUntilExpiry);

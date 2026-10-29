namespace Innovayse.Hostpanel.Application.Admin.Integrations.Common;

/// <summary>The stored state of one integration field, as the admin form reads it.</summary>
/// <param name="Value">The stored value; always <c>null</c> for a secret field.</param>
/// <param name="IsSecret">Whether the field holds a secret and is write only.</param>
/// <param name="HasValue">Whether a non-blank value is stored.</param>
public sealed record IntegrationFieldValueDto(string? Value, bool IsSecret, bool HasValue);

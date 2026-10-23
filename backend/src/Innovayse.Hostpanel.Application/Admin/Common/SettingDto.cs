namespace Innovayse.Hostpanel.Application.Admin.Common;

/// <summary>
/// Data transfer object representing a system configuration setting.
/// </summary>
/// <remarks>
/// A secret setting (see <see cref="SettingSecrecy"/>) never carries its value: <see cref="Value"/>
/// is <c>null</c> and only <see cref="IsSecret"/> and <see cref="HasValue"/> describe it.
/// </remarks>
/// <param name="Id">The setting's primary key.</param>
/// <param name="Key">The unique configuration key.</param>
/// <param name="Value">The current configuration value; <c>null</c> for a secret setting.</param>
/// <param name="Description">Optional human-readable description of this setting.</param>
/// <param name="IsSecret">Whether the setting holds a secret and is write only.</param>
/// <param name="HasValue">Whether a non-blank value is stored.</param>
public record SettingDto(int Id, string Key, string? Value, string? Description, bool IsSecret, bool HasValue);

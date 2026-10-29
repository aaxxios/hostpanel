namespace Innovayse.Hostpanel.Domain.Settings;

/// <summary>A setting key with the value it is seeded with and the description shown in System Settings.</summary>
/// <param name="Key">The setting key.</param>
/// <param name="Value">The seeded value.</param>
/// <param name="Description">The description shown in System Settings.</param>
public sealed record SettingDefault(string Key, string Value, string Description);

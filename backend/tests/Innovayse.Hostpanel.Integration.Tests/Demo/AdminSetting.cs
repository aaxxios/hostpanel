namespace Innovayse.Hostpanel.Integration.Tests.Demo;

/// <summary>
/// An administrator client and the id of the setting it edits.
/// </summary>
/// <param name="Client">Signed in as an administrator.</param>
/// <param name="SettingId">The setting's id.</param>
public sealed record AdminSetting(HttpClient Client, int SettingId);

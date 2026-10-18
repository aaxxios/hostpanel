namespace Innovayse.Hostpanel.Infrastructure.Auth.Settings;

/// <summary>
/// The shared key that authenticates the platform portal's sign-out fan-out. Bound from the
/// <c>BackchannelLogout</c> section (<c>BackchannelLogout__Key</c> in compose). Required when
/// <c>Auth:Mode</c> is <c>sso</c>; optional otherwise, where an unset key refuses every call.
/// </summary>
public sealed class BackchannelLogoutOptions
{
    /// <summary>The configuration section these options bind from.</summary>
    public const string SectionName = "BackchannelLogout";

    /// <summary>The key the caller presents in <c>X-Logout-Key</c>; empty means every call is refused.</summary>
    public string Key { get; init; } = string.Empty;
}

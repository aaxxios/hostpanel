namespace Innovayse.Hostpanel.Shared.Settings;

/// <summary>Who owns this deployment's people: the product itself (local) or an external sign-on service.</summary>
public sealed class AuthModeOptions
{
    /// <summary>The configuration section these options bind from.</summary>
    public const string SectionName = "Auth";

    /// <summary>The configured mode, <c>local</c> or <c>sso</c>; anything but <c>local</c> is the external sign-on mode.</summary>
    public string? Mode { get; init; }
}

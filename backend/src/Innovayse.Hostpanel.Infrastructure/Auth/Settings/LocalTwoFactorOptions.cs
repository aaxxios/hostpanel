namespace Innovayse.Hostpanel.Infrastructure.Auth.Settings;

/// <summary>
/// Settings for local-mode two-factor enrolment. Not a section: the issuer comes from the bare
/// top-level <c>AppName</c> key, which a self-hosted deployment already sets for its own name.
/// </summary>
public sealed class LocalTwoFactorOptions
{
    /// <summary>The top-level configuration key the issuer is read from.</summary>
    public const string ConfigurationKey = "AppName";

    /// <summary>
    /// The name an authenticator app shows beside the account. A deployment sets its own name; the
    /// neutral product name is used when it sets none.
    /// </summary>
    public string Issuer { get; set; } = "Hostpanel";
}

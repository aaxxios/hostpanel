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
    /// The name an authenticator app shows beside the account. A self-hosted deployment's own name,
    /// so its users' QR codes do not carry this product's; the platform's own deployment never sets it.
    /// </summary>
    public string Issuer { get; set; } = "Innovayse";
}

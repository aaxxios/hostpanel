namespace Innovayse.Hostpanel.Shared.Settings;

/// <summary>The external sign-on service whose tokens the API accepts when it does not own its people.</summary>
public sealed class SsoOptions
{
    /// <summary>The configuration section these options bind from.</summary>
    public const string SectionName = "Sso";

    /// <summary>The issuer's base URL, where its discovery document and signing keys are read from.</summary>
    public string? Authority { get; init; }

    /// <summary>The client id this API is registered under at the issuer, used as the expected audience.</summary>
    public string? ClientId { get; init; }

    /// <summary>This product's own credential for the issuer's service API, sent on calls that read people.</summary>
    public string? ServiceKey { get; init; }
}

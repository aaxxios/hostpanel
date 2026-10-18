namespace Innovayse.Hostpanel.Infrastructure.Auth.Settings;

/// <summary>
/// The local token signer's settings, bound from the <c>Jwt</c> section. Read by the signer
/// (<see cref="JwtTokenService"/>) and by the composition root's token validators from this one
/// class, so a signer and a validator can never disagree about a key.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// HMAC-SHA256 signing secret, at least 32 characters. Unset only on a developer machine, where
    /// <see cref="JwtTokenService.DevSecretFallback"/> stands in; local mode refuses to start with a
    /// shorter value.
    /// </summary>
    public string? Secret { get; set; }

    /// <summary>The <c>iss</c> claim written and required. A plain identifier, safe to default.</summary>
    public string Issuer { get; set; } = JwtTokenService.DefaultIssuer;

    /// <summary>The <c>aud</c> claim written and required. A plain identifier, safe to default.</summary>
    public string Audience { get; set; } = JwtTokenService.DefaultAudience;

    /// <summary>The secret actually used: <see cref="Secret"/>, or the developer fallback when unset.</summary>
    public string SigningSecret => Secret ?? JwtTokenService.DevSecretFallback;
}

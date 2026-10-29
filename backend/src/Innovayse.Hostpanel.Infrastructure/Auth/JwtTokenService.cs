using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Infrastructure.Auth.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// Issues local JWTs when Auth:Mode=local (no external sign-on provider dependency).
/// </summary>
/// <remarks>
/// Lives in Infrastructure: signing a token is I/O-adjacent work behind the
/// <see cref="IJwtService"/> port, so callers depend on the contract, not on this class.
/// </remarks>
public sealed class JwtTokenService : IJwtService
{
    /// <summary>The signer's settings. The composition root builds its token *validators* from the same <see cref="JwtOptions"/> class, so a signer and a validator cannot disagree about a key.</summary>
    private readonly IOptions<JwtOptions> _options;

    /// <summary>Session lifetimes: token length and absolute maximum age.</summary>
    private readonly IOptions<SessionOptions> _session;

    /// <summary>Clock, replaceable so expiry can be tested.</summary>
    private readonly TimeProvider _time;

    /// <summary>Initializes a new instance of the <see cref="JwtTokenService"/> class.</summary>
    /// <param name="options">The signer's settings. The composition root builds its token *validators* from the same <see cref="JwtOptions"/> class, so a signer and a validator cannot disagree about a key.</param>
    /// <param name="session">Session lifetimes: token length and absolute maximum age.</param>
    /// <param name="time">Clock, replaceable so expiry can be tested.</param>
    public JwtTokenService(IOptions<JwtOptions> options, IOptions<SessionOptions> session, TimeProvider time)
    {
        _options = options;
        _session = session;
        _time = time;
    }

    /// <summary>
    /// Dev-only fallback signing secret, used when Jwt:Secret is unset. Program.cs applies
    /// this same fallback for validating incoming tokens: shared here as the one place both
    /// sides read from, so a token this service signs is never rejected by the validator that's
    /// supposed to accept it (or, unset, throws instead of silently signing with two different
    /// keys). Never set in a deployed environment; validated there via Program.cs's length check.
    /// </summary>
    public const string DevSecretFallback = "change-this-to-a-32-char-min-secret-key-here";

    /// <summary>
    /// Default issuer/audience, used whenever Jwt:Issuer / Jwt:Audience are unset. Unlike
    /// <see cref="DevSecretFallback"/> these are fine to run with in any environment, they're
    /// just identifier strings, not a secret, but they still have to be the exact values
    /// Program.cs's token validators fall back to, or a token minted without either config key
    /// set carries issuer/audience: null and gets rejected by validators expecting these
    /// defaults, which is a 401 on every request with an otherwise-valid token.
    /// </summary>
    public const string DefaultIssuer = "innovayse-api";

    /// <summary>See <see cref="DefaultIssuer"/>.</summary>
    public const string DefaultAudience = "innovayse-clients";

    /// <summary>Claim holding the Unix time of the original sign-in; kept across renewals so the absolute maximum age can be enforced.</summary>
    public const string SessionStartClaim = "session_start";

    /// <summary>
    /// Claim holding the account's session version at issue; carried unchanged across renewals, so
    /// a renewed token is refused as soon as the account's version moves on.
    /// </summary>
    public const string SessionVersionClaim = "session_version";

    /// <summary>A token is renewed once its remaining life is at most this fraction (1/N) of its lifetime.</summary>
    private const int RenewalWindowDivisor = 3;

    /// <inheritdoc/>
    public TimeSpan AccessTokenLifetime => TimeSpan.FromMinutes(_session.Value.AccessTokenMinutes);

    /// <summary>Number of random bytes behind a refresh token, before base64 encoding.</summary>
    private const int RefreshTokenBytes = 32;

    /// <inheritdoc/>
    public string GenerateAccessToken(AccessTokenClaims claims)
    {
        ArgumentNullException.ThrowIfNull(claims);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.SigningSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenClaims = new List<Claim>
        {
            new(Innovayse.Hostpanel.Shared.Constants.JwtClaims.Sub, claims.UserId),
            new("email", claims.Email),
            new("email_verified", claims.EmailVerified ? "true" : "false"),
            new(SessionVersionClaim, claims.SessionVersion),
            new(SessionStartClaim, _time.GetUtcNow().ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture)),
        };
        if (!string.IsNullOrEmpty(claims.FirstName))
        {
            tokenClaims.Add(new("given_name", claims.FirstName));
        }

        if (!string.IsNullOrEmpty(claims.LastName))
        {
            tokenClaims.Add(new("family_name", claims.LastName));
        }

        foreach (var role in claims.Roles)
        {
            tokenClaims.Add(new(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _options.Value.Issuer,
            audience: _options.Value.Audience,
            claims: tokenClaims,
            expires: _time.GetUtcNow().UtcDateTime.Add(AccessTokenLifetime),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc/>
    public string? RenewAccessToken(string accessToken)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.SigningSecret));
        var handler = new JwtSecurityTokenHandler();

        SecurityToken validated;
        try
        {
            // Zero skew and the injected clock: renewal must never extend a token that has
            // already expired.
            handler.ValidateToken(accessToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidIssuer = _options.Value.Issuer,
                ValidAudience = _options.Value.Audience,
                ClockSkew = TimeSpan.Zero,
                LifetimeValidator = (_, expires, _, _) => expires is not null && expires > _time.GetUtcNow().UtcDateTime,
            }, out validated);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            return null;
        }

        var jwt = (JwtSecurityToken)validated;

        // A token with no session version cannot be tied to the account's current sessions, so
        // it is never extended. Whether the version is still current is decided by authentication,
        // which runs before any renewal and refuses a stale one.
        if (string.IsNullOrEmpty(jwt.Claims.FirstOrDefault(c => c.Type == SessionVersionClaim)?.Value))
        {
            return null;
        }
        var startClaim = jwt.Claims.FirstOrDefault(c => c.Type == SessionStartClaim)?.Value;
        if (!long.TryParse(startClaim, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var startSeconds))
        {
            // A token minted before renewal existed has no sign-in time to bound it by.
            return null;
        }

        var now = _time.GetUtcNow();
        var lifetime = AccessTokenLifetime;
        var hardEnd = DateTimeOffset.FromUnixTimeSeconds(startSeconds).AddMinutes(_session.Value.MaxSessionMinutes);
        var remaining = new DateTimeOffset(jwt.ValidTo, TimeSpan.Zero) - now;
        if (now >= hardEnd || remaining > lifetime / RenewalWindowDivisor)
        {
            return null;
        }

        var expires = now.Add(lifetime) < hardEnd ? now.Add(lifetime) : hardEnd;
        var carried = jwt.Claims.Where(c => c.Type is not ("exp" or "nbf" or "iat"));
        var renewed = new JwtSecurityToken(
            issuer: _options.Value.Issuer,
            audience: _options.Value.Audience,
            claims: carried,
            expires: expires.UtcDateTime,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return handler.WriteToken(renewed);
    }

    /// <inheritdoc/>
    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(RefreshTokenBytes));
    }
}

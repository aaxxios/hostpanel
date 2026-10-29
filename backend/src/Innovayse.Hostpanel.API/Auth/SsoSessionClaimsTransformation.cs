#if INNOVAYSE_SSO
using System.Security.Claims;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication;

namespace Innovayse.Hostpanel.API.Auth;

/// <summary>
/// Adds this application's roles to a cookie-session principal.
/// </summary>
/// <remarks>
/// The cookie handler is a shared library and knows nothing about this product's roles,
/// so they are attached here: <see cref="IClaimsTransformation"/> runs after any scheme
/// succeeds, and this one acts only on the cookie scheme.
///
/// Without it, an admin signing in through the cookie flow authenticates and is then
/// refused by every <c>AdminOnly</c> policy: the principal would carry the external sign-on provider's claims
/// and none of the roles this database grants.
///
/// <para>
/// It attaches roles and nothing else: it keeps no local copy of the external sign-on provider user and leaves
/// <see cref="ClaimTypes.NameIdentifier"/> as issued, because the external sign-on provider subject is the identifier
/// this product uses throughout.
/// </para>
/// </remarks>
public sealed class SsoSessionClaimsTransformation : IClaimsTransformation
{
    /// <summary>Marker claim type recording that a principal already went through this transformation.</summary>
    private const string MappedClaimType = "hostpanel:mapped";

    /// <summary>The role store whose grants are merged onto the session principal.</summary>
    private readonly ISubjectRoleStore _roles;

    /// <summary>Initializes a new instance of the <see cref="SsoSessionClaimsTransformation"/> class.</summary>
    /// <param name="roles">The role store whose grants are merged onto the session principal.</param>
    public SsoSessionClaimsTransformation(ISubjectRoleStore roles)
    {
        _roles = roles;
    }

    /// <inheritdoc />
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity
            || identity.AuthenticationType != global::Innovayse.Auth.Authentication.CookieSessionHandler.SchemeName)
        {
            return principal;
        }

        // Already mapped on a previous middleware pass of the same request.
        if (identity.HasClaim(c => c.Type == MappedClaimType))
        {
            return principal;
        }

        var subject = principal.FindFirst(Innovayse.Hostpanel.Shared.Constants.JwtClaims.Sub)?.Value;
        if (subject is null)
        {
            return principal;
        }

        foreach (var role in await _roles.GetRolesAsync(subject, CancellationToken.None))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
        }

        identity.AddClaim(new Claim(MappedClaimType, "1"));
        return principal;
    }
}

#endif

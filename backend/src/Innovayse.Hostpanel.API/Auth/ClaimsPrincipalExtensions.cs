using System.Security.Claims;
using Innovayse.Hostpanel.Shared.Constants;

namespace Innovayse.Hostpanel.API.Auth;

/// <summary>Reads the caller's subject off a principal, the one place both claim spellings are known.</summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The subject of the principal. Both claim names, because the two modes deliver it differently:
    /// the external sign-on mode keeps <c>sub</c> as itself, local mode maps it to
    /// <see cref="ClaimTypes.NameIdentifier"/>.
    /// </summary>
    /// <param name="principal">The authenticated caller, under either claim spelling.</param>
    /// <returns>The subject, or null when the principal carries neither claim.</returns>
    public static string? GetSubject(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtClaims.Sub);
    }
}

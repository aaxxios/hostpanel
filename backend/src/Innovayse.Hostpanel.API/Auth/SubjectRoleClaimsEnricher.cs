using System.Security.Claims;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Innovayse.Hostpanel.API.Auth;

/// <summary>
/// Merges the roles held in <see cref="ISubjectRoleStore"/> onto a validated bearer token,
/// so authorization asks one store whatever minted the credential.
/// </summary>
/// <remarks>
/// <para>
/// Every grant in this product writes to <c>SubjectRoles</c>: <c>AuthController.SetupAsync</c>,
/// <c>AdminCreateClientHandler</c>, <c>PlaceOrderHandler</c>, <c>AcceptInvitationHandler</c> and
/// <c>MigrationPullWorker</c> all call <see cref="ISubjectRoleStore.AddAsync"/>. A credential this
/// product issued itself carries only the roles baked into it at sign-in, so without this merge a
/// role granted after the token was minted, which is every grant listed above, would never reach an
/// role-restricted <c>[Authorize]</c> route, and the Admin row <c>POST /api/auth/setup</c> writes would
/// authorize nothing while <c>setup-required</c> answers <c>false</c> for good.
/// </para>
/// <para>
/// It is attached to every JWT scheme in both modes, so authorization is decided by
/// <c>SubjectRoles</c> in both. It is deliberately mode-agnostic: it knows about a role store, not about who issued the token, so
/// it sits above the <c>AUTH_MODE</c> port pair rather than inside either implementation.
/// </para>
/// <para>
/// Roles already on the token are kept rather than replaced. A token minted with a role the
/// store has not been told about is still a credential this product issued, and dropping its
/// claims here would revoke access as a side effect of reading a second source.
/// </para>
/// </remarks>
public static class SubjectRoleClaimsEnricher
{
    /// <summary>
    /// Builds the <see cref="JwtBearerEvents.OnTokenValidated"/> handler that adds the subject's
    /// stored roles to the validated principal.
    /// </summary>
    /// <returns>A handler suitable for assignment to <see cref="JwtBearerEvents.OnTokenValidated"/>.</returns>
    public static Func<TokenValidatedContext, Task> OnTokenValidated() => EnrichAsync;

    /// <summary>
    /// Adds every role the subject holds in the store as a <see cref="ClaimTypes.Role"/> claim.
    /// </summary>
    /// <param name="context">The token-validation context carrying the principal to enrich.</param>
    /// <returns>A task that completes once the roles have been merged.</returns>
    private static async Task EnrichAsync(TokenValidatedContext context)
    {
        // Both spellings, because the two schemes deliver the subject differently: external sign-on mode sets
        // MapInboundClaims = false so "sub" survives as itself, while the locally issued schemes
        // leave the default mapping on, which renames it to NameIdentifier. Reading only one name
        // finds nothing on the other scheme: the same trap HttpCurrentUserService documents.
        var subject = context.Principal?.GetSubject();

        if (string.IsNullOrEmpty(subject))
        {
            return;
        }

        if (context.Principal!.Identity is not ClaimsIdentity identity)
        {
            return;
        }

        var store = context.HttpContext.RequestServices.GetRequiredService<ISubjectRoleStore>();
        var held = await store.GetRolesAsync(subject, context.HttpContext.RequestAborted);

        foreach (var role in held.Select(r => r.ToString()))
        {
            // Guarded because a token may already carry the role it was minted with, and a
            // duplicate role claim makes IsInRole answer twice for one grant: harmless for
            // authorization but visible to anything that enumerates the principal's claims.
            if (!identity.HasClaim(ClaimTypes.Role, role))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }
    }
}

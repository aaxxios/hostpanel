using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Innovayse.Hostpanel.API.Auth;

/// <summary>
/// Refuses a local access token whose session version is no longer the account's current one.
/// </summary>
/// <remarks>
/// <para>
/// A local token is stateless, so on its own it stays good until it expires, and sliding renewal
/// would keep extending it. Each token carries the session version it was issued with; the
/// version moves on when the password is reset, two-factor is switched off or the sign-in
/// address changes, and from then on every token issued before is refused here. Sliding renewal
/// runs after authentication, so a refused token is never renewed either.
/// </para>
/// <para>
/// Attached only to local mode's scheme: that is the only mode in which this deployment owns the
/// accounts, signs people in with a password and registers <see cref="IUserService"/>.
/// </para>
/// </remarks>
public static class LocalSessionVersionValidator
{
    /// <summary>The reason recorded on a refused token; never shown to the caller.</summary>
    private const string SessionEndedReason = "The session this token belongs to has ended.";

    /// <summary>
    /// Builds the <see cref="JwtBearerEvents.OnTokenValidated"/> handler that checks the session
    /// version first and, when the token is still current, merges the subject's stored roles.
    /// </summary>
    /// <returns>A handler suitable for assignment to <see cref="JwtBearerEvents.OnTokenValidated"/>.</returns>
    public static Func<TokenValidatedContext, Task> ThenEnrichRoles()
    {
        var enrich = SubjectRoleClaimsEnricher.OnTokenValidated();
        return async context =>
        {
            await ValidateAsync(context);
            if (context.Result is null)
            {
                await enrich(context);
            }
        };
    }

    /// <summary>Fails the token when its session version is missing or no longer current.</summary>
    /// <param name="context">The token-validation context.</param>
    /// <returns>A task that completes once the token has been accepted or failed.</returns>
    private static async Task ValidateAsync(TokenValidatedContext context)
    {
        var subject = context.Principal?.GetSubject();
        var presented = context.Principal?.FindFirst(JwtTokenService.SessionVersionClaim)?.Value;
        if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(presented))
        {
            context.Fail(SessionEndedReason);
            return;
        }

        var users = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
        var current = await users.GetSessionVersionAsync(subject, context.HttpContext.RequestAborted);
        if (!string.Equals(current, presented, StringComparison.Ordinal))
        {
            context.Fail(SessionEndedReason);
        }
    }
}

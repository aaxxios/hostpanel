using System.Security.Claims;
using Innovayse.Hostpanel.Shared.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Innovayse.Hostpanel.API.Auth;

/// <summary>
/// Accepts an external sign-on provider token only when it was issued to this deployment's own client.
/// </summary>
/// <remarks>
/// The provider stamps its access tokens with the resource servers they are valid at, not with the
/// client that asked for them, so the audience alone does not tell this API whether a token was
/// meant for it. The client the token was issued to does: a token another application obtained
/// from the same provider is refused here even though its signature and issuer are valid.
/// </remarks>
public static class SignOnClientBinding
{
    /// <summary>Whether the principal's token was issued to <paramref name="clientId"/>.</summary>
    /// <param name="principal">The validated principal.</param>
    /// <param name="clientId">The client id this deployment is registered under at the provider.</param>
    /// <returns>True when the token's client, or its authorized party, is <paramref name="clientId"/>.</returns>
    public static bool IsIssuedTo(ClaimsPrincipal? principal, string clientId)
    {
        if (principal is null)
        {
            return false;
        }

        var issuedTo = principal.FindFirst(JwtClaims.ClientId)?.Value
            ?? principal.FindFirst(JwtClaims.AuthorizedParty)?.Value;

        return string.Equals(issuedTo, clientId, StringComparison.Ordinal);
    }

    /// <summary>
    /// Builds the <see cref="JwtBearerEvents.OnTokenValidated"/> handler that refuses a token issued to
    /// another client and otherwise hands over to <paramref name="next"/>.
    /// </summary>
    /// <param name="clientId">The client id this deployment is registered under at the provider.</param>
    /// <param name="next">The handler to run for a token that passed the check.</param>
    /// <returns>A handler suitable for assignment to <see cref="JwtBearerEvents.OnTokenValidated"/>.</returns>
    public static Func<TokenValidatedContext, Task> OnTokenValidated(
        string clientId,
        Func<TokenValidatedContext, Task> next)
    {
        return context =>
        {
            if (!IsIssuedTo(context.Principal, clientId))
            {
                context.Fail("The token was not issued to this application.");
                return Task.CompletedTask;
            }

            return next(context);
        };
    }
}

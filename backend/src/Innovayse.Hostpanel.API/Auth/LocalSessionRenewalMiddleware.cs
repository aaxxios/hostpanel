using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Infrastructure;

namespace Innovayse.Hostpanel.API.Auth;

/// <summary>
/// Sliding renewal of the local session: a valid token in the last third of its lifetime is
/// answered with a fresh one, so a person who keeps working is not signed out every 15 minutes.
/// </summary>
/// <remarks>
/// <para>
/// A browser caller (session cookie) gets the cookie re-issued; a machine caller (the client
/// portal's Nuxt server, bearer header) gets the new token in <see cref="RenewedTokenHeader"/>
/// for its own cookie. The renewal itself, including the absolute max session age, lives behind
/// <see cref="IJwtService.RenewAccessToken"/>.
/// </para>
/// <para>
/// Registered only in <c>Program.cs</c>'s local branch, after authentication. External sign-on is untouched.
/// Running after authentication is what keeps a revoked session from being extended: a token whose
/// session version is no longer the account's own is refused by
/// <see cref="LocalSessionVersionValidator"/>, the request is then not authenticated, and nothing
/// here renews it. The presented token is read the same way authentication reads it (the header
/// first, the cookie otherwise), so the token renewed is always the one that was just checked.
/// </para>
/// </remarks>
public sealed class LocalSessionRenewalMiddleware
{
    /// <summary>Response header carrying a renewed token to a bearer caller.</summary>
    public const string RenewedTokenHeader = "X-Renewed-Access-Token";

    /// <summary>Path of the sign-out action; a renewal must never undo the cookie it clears.</summary>
    private const string LogoutPath = "/api/auth/logout";

    /// <summary>The next middleware in the pipeline.</summary>
    private readonly RequestDelegate _next;

    /// <summary>Initializes a new instance of the <see cref="LocalSessionRenewalMiddleware"/> class.</summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public LocalSessionRenewalMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>Renews the presented token when it is due, then continues the pipeline.</summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="tokens">The token service that decides whether a renewal is due.</param>
    /// <returns>A task that completes when the rest of the pipeline has run.</returns>
    public Task InvokeAsync(HttpContext context, IJwtService tokens)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && !context.Request.Path.StartsWithSegments(LogoutPath, StringComparison.OrdinalIgnoreCase))
        {
            var fromHeader = BearerHeader.TryRead(context.Request, out var bearerToken);
            var presented = fromHeader
                ? bearerToken
                : context.Request.Cookies[LocalSessionCookie.Name];

            var renewed = string.IsNullOrEmpty(presented) ? null : tokens.RenewAccessToken(presented);
            if (renewed is not null)
            {
                // Written when the response starts, and only for a success: a refused request
                // must not hand out a credential.
                context.Response.OnStarting(() =>
                {
                    if (context.Response.StatusCode < StatusCodes.Status400BadRequest)
                    {
                        if (fromHeader)
                        {
                            context.Response.Headers[RenewedTokenHeader] = renewed;
                        }
                        else
                        {
                            LocalSessionCookie.Issue(context, renewed);
                        }
                    }

                    return Task.CompletedTask;
                });
            }
        }

        return _next(context);
    }
}

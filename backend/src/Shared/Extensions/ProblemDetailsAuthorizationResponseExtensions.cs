using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Net.Http.Headers;

namespace Innovayse.Hostpanel.Shared.Extensions;

/// <summary>
/// Guarantees a localized <see cref="ErrorResponse"/> body for every 401/403 the authorization
/// middleware answers, regardless of which scheme protected the endpoint.
/// </summary>
/// <remarks>
/// <para>
/// A <c>JwtBearerEvents</c> hook was written for this first and then deleted: it needed a
/// <c>Microsoft.AspNetCore.Authentication.JwtBearer</c> reference the <c>Shared</c> project does not
/// carry, and it turned out to be redundant — this handler already answers both
/// <c>Challenged</c> (401) and <c>Forbidden</c> (403). It also could not have covered two refusals
/// this product produces:
/// the future <c>InternalEndpointAuthMiddleware</c> (phase 7, <c>Innovayse.Hostpanel.API</c>) will
/// derive from the plain <c>AuthenticationHandler&lt;AuthenticationSchemeOptions&gt;</c>, which has
/// no <c>Events</c> object at all to hook — its default challenge and forbid are a bare status line
/// with no body, full stop. And the cookie scheme's default forbid is a redirect to an access-denied
/// page, which is right for a browser navigating a page and wrong for an API caller.
/// </para>
/// <para>
/// Both of those are written by <c>AuthorizationMiddleware</c> calling
/// <see cref="IAuthorizationMiddlewareResultHandler.HandleAsync"/>, not by a scheme's own handler
/// directly, so the fix sits one layer up: decorate the default result handler, let it do whatever
/// it already does (including, for the <c>Bearer</c> scheme, triggering the <c>JwtBearerEvents</c>
/// hook above), and only step in when nothing wrote a body. Because every policy in this product —
/// <c>Member</c>, <c>OrgAdmin</c>, <c>PlatformAdmin</c> and <c>InternalService</c> — is evaluated by
/// the same middleware, one registration covers all four instead of one hook per scheme.
/// </para>
/// </remarks>
public static class ProblemDetailsAuthorizationResponseExtensions
{
    /// <summary>Registers the decorator described above.</summary>
    /// <param name="services">The container. Call once, after the policies are declared —
    /// registration order does not change what this decorates, but keeping it beside
    /// <c>AddAuthorizationBuilder()</c> calls is where a reader expects to find it.</param>
    /// <returns>The container, for chaining.</returns>
    public static IServiceCollection AddProblemDetailsAuthorizationResponses(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddSingleton<
            IAuthorizationMiddlewareResultHandler, ProblemDetailsAuthorizationMiddlewareResultHandler>();
    }

    /// <summary>
    /// Decorates the framework's default <see cref="IAuthorizationMiddlewareResultHandler"/>: runs
    /// it first, then writes the product's own <see cref="ErrorResponse"/> body if the default
    /// handling left the response unwritten.
    /// </summary>
    private sealed class ProblemDetailsAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        /// <summary>
        /// The stock handler this class wraps. Stateless and safe to share: it holds no per-request
        /// data, only the logic that calls <c>ChallengeAsync</c>/<c>ForbidAsync</c> on the resolved
        /// scheme.
        /// </summary>
        private static readonly AuthorizationMiddlewareResultHandler DefaultHandler = new();

        /// <inheritdoc />
        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(authorizeResult);

            await DefaultHandler.HandleAsync(next, context, policy, authorizeResult);

            // A scheme's own handler may already have written during the call above; a flushed
            // response cannot be modified and trying throws, so this guards against double-writing
            // as well as against a response flushed for an unrelated reason (streaming, a
            // websocket upgrade).
            if (context.Response.HasStarted)
            {
                return;
            }

            if (authorizeResult.Forbidden)
            {
                await WriteAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    SharedErrorCodes.Forbidden,
                    "Forbidden",
                    "ForbiddenDetail");
            }
            else if (authorizeResult.Challenged)
            {
                await WriteAsync(
                    context,
                    StatusCodes.Status401Unauthorized,
                    SharedErrorCodes.Unauthorized,
                    "Unauthorized",
                    "UnauthorizedDetail");
            }
        }

        /// <summary>Writes the localized <see cref="ErrorResponse"/> the default handler left unwritten.</summary>
        /// <param name="context">The current request.</param>
        /// <param name="status">401 or 403.</param>
        /// <param name="code">The registry code the status is answered with.</param>
        /// <param name="titleKey">The resource key for the title.</param>
        /// <param name="detailKey">The resource key for the detail.</param>
        /// <returns>A task that completes when the response is written.</returns>
        private static async Task WriteAsync(
            HttpContext context, int status, string code, string titleKey, string detailKey)
        {
            // A scheme whose default outcome is a redirect (the cookie scheme's access-denied page)
            // sets Location without writing a body, so HasStarted is still false here and this would
            // otherwise ship a stray header alongside the JSON body below.
            context.Response.Headers.Remove(HeaderNames.Location);

            var localizer = context.RequestServices.GetRequiredService<IStringLocalizer<ValidationMessages>>();
            var body = new ErrorResponse(code, localizer[titleKey], status, localizer[detailKey]);

            await ErrorBody.WriteAsync(context, body, context.RequestAborted);
        }
    }
}

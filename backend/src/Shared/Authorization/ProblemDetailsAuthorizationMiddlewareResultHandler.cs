using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Net.Http.Headers;

namespace Innovayse.Hostpanel.Shared.Authorization;

/// <summary>
/// Decorates the framework's default <see cref="IAuthorizationMiddlewareResultHandler"/>: runs
/// it first, then writes the product's own <see cref="ErrorResponse"/> body if the default
/// handling left the response unwritten.
/// </summary>
public sealed class ProblemDetailsAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
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

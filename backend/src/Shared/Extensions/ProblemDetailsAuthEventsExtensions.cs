using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.Resources;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Shared.Extensions;

/// <summary>
/// Makes JWT authentication failures answer with an RFC 9457 ProblemDetails body instead of the
/// framework default, which writes a bare status line with <c>Content-Length: 0</c>.
/// </summary>
/// <remarks>
/// <para>
/// The empty body is invisible to a client: the frontend reads <c>type</c>/<c>detail</c> out of
/// the response, so an unauthenticated call was indistinguishable from a transport failure, and
/// those are the two cases the client has *different* sentences for. Every other error path goes
/// through <see cref="Middleware.ExceptionHandlingMiddleware"/> and emits the same
/// <see cref="ErrorResponse"/> with <c>application/problem+json</c>; the authentication handler
/// short-circuits before that middleware can run, so it needs its own hook.
/// </para>
/// <para>
/// The writer attaches from an <c>AddJwtBearer(options =&gt; { })</c> callback where a product has one,
/// and through <c>PostConfigure</c> where it does not: this product does not, because its Bearer
/// scheme is registered inside the shared authentication package. Either way it runs after whatever
/// that package configured, which is why <c>Events</c> is reused rather than replaced: assigning a
/// fresh <see cref="JwtBearerEvents"/> would silently drop any handler already attached.
/// </para>
/// <para>
/// This covers the Bearer scheme's own challenge and forbid. It cannot cover two other refusals
/// this product produces, which is why
/// <see cref="ProblemDetailsAuthorizationResponseExtensions"/> exists alongside it: the
/// <c>X-Service-Key</c> scheme's handler derives from the plain
/// <c>AuthenticationHandler&lt;AuthenticationSchemeOptions&gt;</c> and has no <c>Events</c> object
/// to hook at all, and an authorization *policy* failure is written by the authorization
/// middleware rather than by any scheme. Both writers guard on
/// <see cref="HttpResponse.HasStarted"/>, so whichever runs first wins and the other is a no-op.
/// </para>
/// </remarks>
public static class ProblemDetailsAuthEventsExtensions
{
    /// <summary>
    /// Attaches the ProblemDetails writers for 401 and 403 to these bearer options, preserving
    /// any <see cref="JwtBearerEvents"/> already configured.
    /// </summary>
    /// <remarks>
    /// Written as an extension on the options rather than on the service collection so it can be
    /// called straight from an <c>AddJwtBearer(options =&gt; { options.UseProblemDetailsResponses(); })</c>
    /// callback, which is where a product that registers its own bearer scheme wants it. This
    /// product has no such callback, its scheme comes from the shared authentication package, so
    /// <see cref="ConfigureProblemDetailsAuthEvents"/> reaches the same options through
    /// <c>PostConfigure</c> and calls this. One method, two ways in.
    /// </remarks>
    /// <param name="options">The bearer options being configured.</param>
    public static void UseProblemDetailsResponses(this JwtBearerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var events = options.Events ??= new JwtBearerEvents();

        events.OnChallenge = async context =>
        {
            // Suppress the default handler so it does not overwrite the body written below.
            context.HandleResponse();

            await WriteAsync(
                context.HttpContext,
                StatusCodes.Status401Unauthorized,
                SharedErrorCodes.Unauthorized,
                "Unauthorized",
                "UnauthorizedDetail");
        };

        events.OnForbidden = context => WriteAsync(
            context.HttpContext,
            StatusCodes.Status403Forbidden,
            SharedErrorCodes.Forbidden,
            "Forbidden",
            "ForbiddenDetail");
    }

    /// <summary>
    /// Registers <see cref="UseProblemDetailsResponses"/> against the Bearer scheme for a product
    /// that never calls <c>AddJwtBearer</c> itself.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection ConfigureProblemDetailsAuthEvents(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.PostConfigure<JwtBearerOptions>(
            JwtBearerDefaults.AuthenticationScheme,
            options => options.UseProblemDetailsResponses());
    }

    /// <summary>
    /// Writes an <see cref="ErrorResponse"/> body for an authentication or authorization refusal.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="status">HTTP status to answer with.</param>
    /// <param name="type">Machine-readable error code; never localized, the client branches on it.</param>
    /// <param name="title">Short human-readable title.</param>
    /// <param name="detailKey">Resource key resolved against the request's culture.</param>
    /// <returns>A task that completes once the body has been written, or immediately when there is
    /// nothing safe to write to.</returns>
    private static async Task WriteAsync(
        HttpContext context,
        int status,
        string type,
        string title,
        string detailKey)
    {
        // Headers may already be flushed for a streamed response or a websocket upgrade; writing
        // then throws, turning a handled refusal into an unhandled fault.
        if (context.Response.HasStarted)
        {
            return;
        }

        // Resolved per request so the sentence follows the caller's own X-Language, the same way
        // every message the exception middleware produces does.
        var localizer = context.RequestServices.GetRequiredService<IStringLocalizer<ValidationMessages>>();

        var body = new ErrorResponse(type, title, status, localizer[detailKey]);

        await ErrorBody.WriteAsync(context, body, context.RequestAborted);
    }
}

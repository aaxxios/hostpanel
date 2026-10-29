using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Shared.Extensions;

/// <summary>
/// Guarantees a localized <see cref="ErrorResponse"/> body for every 401/403 the authorization
/// middleware answers, regardless of which scheme protected the endpoint.
/// </summary>
/// <remarks>
/// <para>
/// A per-scheme <c>JwtBearerEvents</c> hook is not used: it needs a
/// <c>Microsoft.AspNetCore.Authentication.JwtBearer</c> reference the <c>Shared</c> project does not
/// carry, and it would not cover every refusal. A scheme built on the plain
/// <c>AuthenticationHandler&lt;AuthenticationSchemeOptions&gt;</c> has no <c>Events</c> object to
/// hook, and its default challenge and forbid are a bare status line with no body. The cookie
/// scheme's default forbid is a redirect to an access-denied page, which is right for a browser
/// navigating a page and wrong for an API caller.
/// </para>
/// <para>
/// All of those are written by <c>AuthorizationMiddleware</c> calling
/// <see cref="IAuthorizationMiddlewareResultHandler.HandleAsync"/>, not by a scheme's own handler
/// directly, so the decorator sits one layer up: it lets the default result handler do whatever it
/// already does and only steps in when nothing wrote a body. Every policy in
/// <see cref="Policies"/> is evaluated by the same middleware, so one registration covers them all
/// instead of one hook per scheme.
/// </para>
/// </remarks>
public static class ProblemDetailsAuthorizationResponseExtensions
{
    /// <summary>Registers the decorator described above.</summary>
    /// <param name="services">The container. Call once, after the policies are declared:
    /// registration order does not change what this decorates, but keeping it beside
    /// <c>AddAuthorizationBuilder()</c> calls is where a reader expects to find it.</param>
    /// <returns>The container, for chaining.</returns>
    public static IServiceCollection AddProblemDetailsAuthorizationResponses(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddSingleton<
            IAuthorizationMiddlewareResultHandler, ProblemDetailsAuthorizationMiddlewareResultHandler>();
    }
}

using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Innovayse.Hostpanel.API.Auth;

/// <summary>
/// Answers 404 to every route it guards unless this deployment owns its own people. Applied to the
/// local sign-in controller, whose routes mean nothing where an external sign-on provider owns the
/// accounts.
/// </summary>
/// <remarks>
/// A resource filter, so the refusal comes before model binding and before the bus: the use cases
/// behind these routes depend on services that exist only in local mode, and this is what keeps them
/// from being resolved anywhere else. The mode is asked of the one shared
/// <see cref="IAuthModeProvider"/>, so this filter always agrees with the rest of the process.
/// </remarks>
public sealed class LocalModeOnlyFilter : IAsyncResourceFilter
{
    /// <summary>Whether this deployment owns its own people.</summary>
    private readonly IAuthModeProvider _authMode;

    /// <summary>Initializes the filter.</summary>
    /// <param name="authMode">Whether this deployment owns its own people.</param>
    public LocalModeOnlyFilter(IAuthModeProvider authMode)
    {
        _authMode = authMode;
    }

    /// <summary>Refuses with the standard 404 body outside local mode; otherwise lets the request through.</summary>
    /// <param name="context">The request about to be routed to an action.</param>
    /// <param name="next">Runs the rest of the pipeline.</param>
    /// <returns>A task that completes when the request has been answered.</returns>
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (!_authMode.IsLocalMode)
        {
            context.Result = ErrorBody.ToActionResult(
                ErrorBody.ForCode(context.HttpContext.RequestServices, SharedErrorCodes.NotFound));
            return;
        }

        await next();
    }
}

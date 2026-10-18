using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Settings;
using Microsoft.Extensions.Options;
using Wolverine;

namespace Innovayse.Hostpanel.Shared.Middleware;

/// <summary>
/// Wolverine middleware that refuses a restricted command while the deployment is a demo. It is
/// attached by <see cref="DemoGuardPolicy"/> only to the chains of <c>IDemoRestricted</c>
/// messages, so it does no type test of its own, and it reads the option on every call so the
/// guard follows the configuration rather than a snapshot taken at startup.
/// </summary>
public class DemoGuardMiddleware
{
    /// <summary>The demo switch.</summary>
    private readonly IOptions<DemoOptions> _options;

    /// <summary>Creates the middleware.</summary>
    /// <param name="options">The demo switch.</param>
    public DemoGuardMiddleware(IOptions<DemoOptions> options)
    {
        _options = options;
    }

    /// <summary>Runs before the restricted command's handler.</summary>
    /// <param name="envelope">The envelope carrying the command.</param>
    /// <exception cref="DemoActionDisabledException">Thrown when the demo is enabled.</exception>
    public void Before(Envelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (_options.Value.Enabled)
        {
            // Throws rather than returning a Result: a Wolverine middleware has no Result return
            // channel to short-circuit the chain, and ExceptionHandlingMiddleware maps this
            // exception to a 403 response.
            throw new DemoActionDisabledException(envelope.Message?.GetType().Name ?? "Command");
        }
    }
}

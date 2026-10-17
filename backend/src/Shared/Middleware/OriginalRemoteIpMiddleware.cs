namespace Innovayse.Hostpanel.Shared.Middleware;

/// <summary>
/// Records the client IP as resolved by the trusted forwarded-headers step, before
/// <c>UseInnovayseAuth</c> runs its own forwarded-headers pass that trusts every proxy. The edge
/// rate limiter partitions anonymous callers on this value, so a forged <c>X-Forwarded-For</c>
/// cannot choose a partition.
/// </summary>
public sealed class OriginalRemoteIpMiddleware
{
    /// <summary>The next middleware.</summary>
    private readonly RequestDelegate _next;

    /// <summary>Initializes a new instance of the <see cref="OriginalRemoteIpMiddleware"/> class.</summary>
    /// <param name="next">The next middleware.</param>
    public OriginalRemoteIpMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>The <see cref="HttpContext.Items"/> key holding the captured address.</summary>
    public const string ItemKey = "Innovayse.Hostpanel.OriginalRemoteIp";

    /// <summary>Stores the current remote address, then continues.</summary>
    /// <param name="context">The request.</param>
    /// <returns>A task that completes when the pipeline has run.</returns>
    public Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Items[ItemKey] = context.Connection.RemoteIpAddress;
        return _next(context);
    }
}

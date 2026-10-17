namespace Innovayse.Hostpanel.Shared.Middleware;

/// <summary>
/// Gives every request a trace id of ours, echoed as <c>X-Request-Id</c> and pushed into the log
/// scope. Ours, not the caller's: an incoming header is a value the caller chose, and a provider's
/// request id is a value we must never surface.
/// </summary>
public sealed class TraceIdMiddleware
{
    /// <summary>The rest of the pipeline.</summary>
    private readonly RequestDelegate _next;

    /// <summary>The logger the scope is opened on.</summary>
    private readonly ILogger<TraceIdMiddleware> _logger;

    /// <summary>Initializes a new instance of the <see cref="TraceIdMiddleware"/> class.</summary>
    /// <param name="next">The rest of the pipeline.</param>
    /// <param name="logger">The logger the scope is opened on.</param>
    public TraceIdMiddleware(RequestDelegate next, ILogger<TraceIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>The <see cref="HttpContext.Items"/> key the trace id is stored under.</summary>
    public const string ItemKey = "TraceId";

    /// <summary>The response header carrying the trace id.</summary>
    public const string HeaderName = "X-Request-Id";

    /// <summary>Assigns the id, sets the header before the response starts, and scopes the log.</summary>
    /// <param name="context">The request.</param>
    /// <returns>A task that completes when the pipeline has run.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Guid.NewGuid().ToString("N");
        context.Items[ItemKey] = traceId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = traceId;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object> { ["trace_id"] = traceId }))
        {
            await _next(context);
        }
    }
}

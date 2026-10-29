using Innovayse.Hostpanel.Shared.MultiTenancy;
using Wolverine;

namespace Innovayse.Hostpanel.Shared.Middleware;

/// <summary>
/// Wolverine middleware that logs message handling with timing information.
/// Includes user context from the current request.
/// Uses <see cref="Envelope"/> to remain message-type-agnostic (Wolverine does not
/// support generic Before/After methods: <c>T</c> would be treated as a DI variable).
/// </summary>
public class LoggingMiddleware
{
    /// <summary>Logger instance for structured log output.</summary>
    private readonly ILogger<LoggingMiddleware> _logger;

    /// <summary>Provides the current authenticated user context.</summary>
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of <see cref="LoggingMiddleware"/>.
    /// </summary>
    /// <param name="logger">Logger instance for structured log output.</param>
    /// <param name="currentUserService">Provides the current authenticated user context.</param>
    public LoggingMiddleware(
        ILogger<LoggingMiddleware> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Runs before the message handler. Logs the message type along with
    /// the current user identifier for traceability.
    /// </summary>
    /// <param name="envelope">Wolverine envelope containing the incoming message.</param>
    /// <returns><see cref="HandlerContinuation.Continue"/> to proceed to the next step in the pipeline.</returns>
    public Task<HandlerContinuation> BeforeAsync(Envelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var requestName = envelope.Message?.GetType().Name ?? "Unknown";

        // Null for a scheduled or broker message, which has no caller.
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Handling {RequestName} for User {UserId}",
            requestName, userId);

        return Task.FromResult(HandlerContinuation.Continue);
    }

    /// <summary>
    /// Runs after the message handler completes successfully.
    /// Logs a confirmation that the message was handled without error.
    /// </summary>
    /// <param name="envelope">Wolverine envelope containing the processed message.</param>
    public void After(Envelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var requestName = envelope.Message?.GetType().Name ?? "Unknown";
        _logger.LogInformation("Handled {RequestName} successfully", requestName);
    }
}

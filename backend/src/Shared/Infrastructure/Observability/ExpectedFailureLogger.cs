using FluentValidation;

namespace Innovayse.Hostpanel.Shared.Infrastructure.Observability;

/// <summary>
/// The logger <see cref="ExpectedFailureLoggerFactory"/> hands out: writes an Error caused by a
/// validation rejection at Debug, and everything else as given.
/// </summary>
public sealed class ExpectedFailureLogger : ILogger
{
    /// <summary>The logger that actually writes.</summary>
    private readonly ILogger _inner;

    /// <summary>Creates the logger.</summary>
    /// <param name="inner">The logger that actually writes.</param>
    public ExpectedFailureLogger(ILogger inner)
    {
        _inner = inner;
    }

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return _inner.BeginScope(state);
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return _inner.IsEnabled(logLevel);
    }

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var level = logLevel == LogLevel.Error && IsExpected(exception) ? LogLevel.Debug : logLevel;
        _inner.Log(level, eventId, state, exception, formatter);
    }

    /// <summary>Whether the exception, or any exception it wraps, is a validation rejection.</summary>
    /// <param name="exception">The logged exception, if any.</param>
    /// <returns><c>true</c> for an expected validation failure.</returns>
    private static bool IsExpected(Exception? exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is ValidationException)
            {
                return true;
            }

            if (current is AggregateException aggregate && aggregate.InnerExceptions.Any(IsExpected))
            {
                return true;
            }
        }

        return false;
    }
}

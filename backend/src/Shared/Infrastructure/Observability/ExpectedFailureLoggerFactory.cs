namespace Innovayse.Hostpanel.Shared.Infrastructure.Observability;

/// <summary>
/// An <see cref="ILoggerFactory"/> decorator whose loggers demote an Error entry to Debug when its
/// exception is, or wraps, a FluentValidation <c>ValidationException</c> — Wolverine's
/// "Invocation of {Message} failed!" line for an expected validation rejection. Everything else
/// passes through unchanged.
/// </summary>
public sealed class ExpectedFailureLoggerFactory : ILoggerFactory
{
    /// <summary>The factory that actually writes.</summary>
    private readonly ILoggerFactory _inner;

    /// <summary>Creates the decorator.</summary>
    /// <param name="inner">The factory that actually writes.</param>
    public ExpectedFailureLoggerFactory(ILoggerFactory inner)
    {
        _inner = inner;
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        return new ExpectedFailureLogger(_inner.CreateLogger(categoryName));
    }

    /// <inheritdoc />
    public void AddProvider(ILoggerProvider provider)
    {
        _inner.AddProvider(provider);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _inner.Dispose();
    }
}

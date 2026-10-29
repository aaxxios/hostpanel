namespace Innovayse.Inecobank.Tests;

/// <summary>No-op <see cref="IDisposable"/> returned by <see cref="CapturingLogger.BeginScope{TState}"/>.</summary>
internal sealed class NullScope : IDisposable
{
    /// <summary>Gets the shared singleton instance.</summary>
    public static readonly NullScope Instance = new();

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}

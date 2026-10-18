using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Persistence;

/// <summary>
/// Scoped <see cref="IDomainEventGate"/>: one flag per unit of work, read by
/// <see cref="DomainEventDispatchInterceptor"/>.
/// </summary>
public sealed class DomainEventGate : IDomainEventGate
{
    /// <summary>How many callers currently hold a suppression; nested callers stack.</summary>
    private int _depth;

    /// <inheritdoc/>
    public bool IsSuppressed => _depth > 0;

    /// <inheritdoc/>
    public IDisposable Suppress()
    {
        _depth++;
        return new Release(this);
    }

    /// <summary>The handle returned by <see cref="Suppress"/>; disposing it lifts one suppression.</summary>
    /// <param name="gate">The gate to release.</param>
    private sealed class Release(DomainEventGate gate) : IDisposable
    {
        /// <summary>Guards against a double dispose lifting someone else's suppression.</summary>
        private bool _disposed;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            gate._depth--;
        }
    }
}

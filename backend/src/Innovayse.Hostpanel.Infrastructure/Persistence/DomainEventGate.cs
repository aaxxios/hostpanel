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
        return new DomainEventGateRelease(this);
    }

    /// <summary>Lifts one suppression; called once by the <see cref="DomainEventGateRelease"/> handle.</summary>
    internal void Lift()
    {
        _depth--;
    }
}

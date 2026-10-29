namespace Innovayse.Hostpanel.Infrastructure.Persistence;

/// <summary>The handle returned by <see cref="DomainEventGate.Suppress"/>; disposing it lifts one suppression.</summary>
internal sealed class DomainEventGateRelease : IDisposable
{
    /// <summary>The gate to release.</summary>
    private readonly DomainEventGate _gate;

    /// <summary>Guards against a double dispose lifting someone else's suppression.</summary>
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="DomainEventGateRelease"/> class.</summary>
    /// <param name="gate">The gate to release.</param>
    public DomainEventGateRelease(DomainEventGate gate)
    {
        _gate = gate;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _gate.Lift();
    }
}

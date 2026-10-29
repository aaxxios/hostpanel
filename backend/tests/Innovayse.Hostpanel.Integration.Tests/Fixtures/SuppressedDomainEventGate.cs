using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An <see cref="IDomainEventGate"/> that is always suppressed, for tests that seed rows through
/// entity factories and then dispatch one event by hand to prove a single handler's effect. Without
/// it the seeding saves would publish their own events and add mail the test did not ask for.
/// </summary>
public sealed class SuppressedDomainEventGate : IDomainEventGate
{
    /// <inheritdoc/>
    public bool IsSuppressed => true;

    /// <inheritdoc/>
    public IDisposable Suppress()
    {
        return new NoRelease();
    }
}

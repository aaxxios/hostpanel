namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A clock a test moves by hand, so token lifetimes can be crossed without waiting.
/// </summary>
public sealed class ManualTimeProvider : TimeProvider
{
    /// <summary>The current instant this clock reports.</summary>
    private DateTimeOffset _now;

    /// <summary>Initializes a new instance of the <see cref="ManualTimeProvider"/> class.</summary>
    /// <param name="start">The instant the clock starts at.</param>
    public ManualTimeProvider(DateTimeOffset start)
    {
        _now = start;
    }

    /// <summary>Moves the clock forward (or back, for a negative span).</summary>
    /// <param name="by">How far to move it.</param>
    public void Advance(TimeSpan by)
    {
        _now += by;
    }

    /// <inheritdoc/>
    public override DateTimeOffset GetUtcNow()
    {
        return _now;
    }
}

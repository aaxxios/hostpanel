using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>In-memory <see cref="IDemoResetSchedule"/>; registered as a singleton so every scope sees the same time.</summary>
internal sealed class DemoResetSchedule : IDemoResetSchedule
{
    /// <summary>Guards <see cref="_nextResetAtTicks"/> against a torn read between the scheduler and a request.</summary>
    private readonly object _gate = new();

    /// <summary>The UTC ticks of the next reset; zero while none is scheduled.</summary>
    private long _nextResetAtTicks;

    /// <inheritdoc/>
    public DateTimeOffset? NextResetAt
    {
        get
        {
            lock (_gate)
            {
                return _nextResetAtTicks == 0 ? null : new DateTimeOffset(_nextResetAtTicks, TimeSpan.Zero);
            }
        }
    }

    /// <inheritdoc/>
    public void Set(DateTimeOffset at)
    {
        lock (_gate)
        {
            _nextResetAtTicks = at.UtcTicks;
        }
    }
}

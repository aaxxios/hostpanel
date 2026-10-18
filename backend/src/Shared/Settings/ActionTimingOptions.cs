using System.ComponentModel.DataAnnotations;

namespace Innovayse.Hostpanel.Shared.Settings;

/// <summary>Whether controller actions are timed, and when one counts as slow enough to log at Warning.</summary>
public sealed class ActionTimingOptions
{
    /// <summary>The configuration section these options bind from.</summary>
    public const string SectionName = "ActionTiming";

    /// <summary>Whether the timing filter logs at all.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>An action taking longer than this many milliseconds is logged at Warning.</summary>
    [Range(1, int.MaxValue)]
    public int SlowThresholdMs { get; init; } = 1000;
}

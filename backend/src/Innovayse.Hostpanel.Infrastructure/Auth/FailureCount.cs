namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>The wrong codes counted for one account inside its window.</summary>
internal sealed class FailureCount
{
    /// <summary>Gets or sets the number of attempts counted so far.</summary>
    public int Count { get; set; }

    /// <summary>Gets the instant the window closes; the next attempt after it starts a new count.</summary>
    public DateTimeOffset WindowEndsAt { get; init; }
}

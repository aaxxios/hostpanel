using System.ComponentModel.DataAnnotations;

namespace Innovayse.Hostpanel.Infrastructure.Auth.Settings;

/// <summary>
/// The per-account cap on wrong one-time codes, bound from the <c>TwoFactorCodeGuard</c> section.
/// It counts across every pending token and session, so opening new challenges does not reset it.
/// </summary>
public sealed class TwoFactorCodeGuardOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "TwoFactorCodeGuard";

    /// <summary>How many wrong codes an account may collect inside one window before every code is refused until the window ends.</summary>
    [Range(1, 100)]
    public int MaxFailures { get; set; } = 10;

    /// <summary>The length of the window, in minutes, counted from the first wrong code.</summary>
    [Range(1, 1440)]
    public int WindowMinutes { get; set; } = 15;
}

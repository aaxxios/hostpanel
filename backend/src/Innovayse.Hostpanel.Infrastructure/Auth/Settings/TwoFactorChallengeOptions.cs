using System.ComponentModel.DataAnnotations;

namespace Innovayse.Hostpanel.Infrastructure.Auth.Settings;

/// <summary>
/// Limits of the pending token a local sign-in hands out between the password and the one-time code,
/// bound from the <c>TwoFactorChallenge</c> section.
/// </summary>
public sealed class TwoFactorChallengeOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "TwoFactorChallenge";

    /// <summary>How long a pending token stays valid after the password step, in seconds.</summary>
    [Range(30, 3600)]
    public int LifetimeSeconds { get; set; } = 300;

    /// <summary>How many codes one challenge accepts before it is dead and the password has to be entered again.</summary>
    [Range(1, 10)]
    public int MaxAttempts { get; set; } = 5;
}

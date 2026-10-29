using System.ComponentModel.DataAnnotations;

namespace Innovayse.Hostpanel.Infrastructure.Auth.Settings;

/// <summary>
/// The account lockout on wrong passwords at local sign-in, bound from the <c>SignInLockout</c>
/// section. The count is kept on the account row, so it holds across every API instance and restart.
/// </summary>
public sealed class SignInLockoutOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "SignInLockout";

    /// <summary>How many wrong passwords in a row lock the account.</summary>
    [Range(1, 100)]
    public int MaxFailedAttempts { get; set; } = 5;

    /// <summary>How long, in minutes, a locked account refuses every password.</summary>
    [Range(1, 1440)]
    public int LockoutMinutes { get; set; } = 15;
}

namespace Innovayse.Hostpanel.Application.Auth.Common.Enums;

/// <summary>
/// How a password sign-in attempt ended.
/// </summary>
public enum PasswordSignInOutcome
{
    /// <summary>The email and password match an account that may sign in.</summary>
    Succeeded = 0,

    /// <summary>The email and password match no account that may sign in.</summary>
    Failed = 1,

    /// <summary>The account is locked out after too many wrong passwords; no password is accepted until the lockout ends.</summary>
    LockedOut = 2,
}

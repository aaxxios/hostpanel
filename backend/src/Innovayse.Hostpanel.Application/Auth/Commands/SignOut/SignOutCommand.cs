namespace Innovayse.Hostpanel.Application.Auth.Commands.SignOut;

/// <summary>
/// Ends this browser's local session. Anonymous and carrying nothing: the session is a cookie the
/// API layer clears, and there is no server-side record of a single local session to revoke.
/// </summary>
public sealed record SignOutCommand;

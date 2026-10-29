namespace Innovayse.Hostpanel.Application.Auth.Commands.Register;

/// <summary>
/// Registers a new local account. Anonymous by nature: it creates the caller's own account, so it
/// names no existing one.
/// </summary>
/// <param name="Email">The new account's email address, which is also its user name.</param>
/// <param name="Password">The password to set.</param>
/// <param name="FirstName">The person's first name.</param>
/// <param name="LastName">The person's last name.</param>
public sealed record RegisterCommand(string Email, string Password, string FirstName, string LastName);

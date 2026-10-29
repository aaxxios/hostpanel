namespace Innovayse.Hostpanel.Application.Clients.Commands.StartMyEmailChange;

/// <summary>
/// Asks to move the caller's sign-in to a new address. Nothing changes until the link mailed to the
/// new address is followed.
/// </summary>
/// <remarks>
/// Whose account is not on the command: the handler asks the credential. The current password is,
/// because a signed-in session alone must not be able to move the account somewhere else.
/// </remarks>
/// <param name="NewEmail">The address to move to.</param>
/// <param name="CurrentPassword">The account's current password. Never logged.</param>
public sealed record StartMyEmailChangeCommand(string NewEmail, string CurrentPassword);

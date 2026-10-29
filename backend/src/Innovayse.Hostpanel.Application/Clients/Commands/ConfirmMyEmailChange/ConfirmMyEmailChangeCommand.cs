namespace Innovayse.Hostpanel.Application.Clients.Commands.ConfirmMyEmailChange;

/// <summary>
/// Makes the caller's address change effective with the token mailed to the new address.
/// </summary>
/// <remarks>
/// Whose account is not on the command: the handler asks the credential, so a link forwarded to
/// someone else changes nothing for them, and a token only ever matches the account it was issued to.
/// </remarks>
/// <param name="Email">The new address, as carried by the link.</param>
/// <param name="Token">The token from the link.</param>
public sealed record ConfirmMyEmailChangeCommand(string Email, string Token);

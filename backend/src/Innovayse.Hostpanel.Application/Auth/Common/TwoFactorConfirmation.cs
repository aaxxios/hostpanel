namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>
/// A code that confirms a pending two-factor setup for one account.
/// </summary>
/// <param name="UserId">Whose account. Comes from the credential, never from a request body.</param>
/// <param name="Code">The six digits the authenticator app currently shows.</param>
public sealed record TwoFactorConfirmation(string UserId, string Code);

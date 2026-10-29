namespace Innovayse.Hostpanel.Application.Auth.Commands.DisableTwoFactor;

/// <summary>Switches two-factor authentication off.</summary>
/// <remarks>
/// Whose account is not on the command at all. The handler asks the credential, so there is
/// no field a caller could set to disarm somebody else's second factor.
/// </remarks>
/// <param name="Code">A current code from the authenticator app.</param>
/// <param name="CurrentPassword">
/// The account's current password, required where this deployment owns the accounts. Never logged.
/// </param>
public sealed record DisableTwoFactorCommand(string Code, string? CurrentPassword = null);

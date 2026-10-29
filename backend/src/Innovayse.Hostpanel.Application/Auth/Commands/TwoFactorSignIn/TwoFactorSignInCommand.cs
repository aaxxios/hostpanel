namespace Innovayse.Hostpanel.Application.Auth.Commands.TwoFactorSignIn;

/// <summary>
/// Finishes a sign-in that stopped at the second factor: the pending token the password step handed
/// out, and the one-time code. Anonymous by nature: the pending token is the proof of who is asking.
/// </summary>
/// <param name="PendingToken">The pending token from the password step; <see langword="null"/> when the body omitted it.</param>
/// <param name="Code">The one-time code; <see langword="null"/> when the body omitted it.</param>
public sealed record TwoFactorSignInCommand(string? PendingToken, string? Code);

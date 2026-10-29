namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>
/// An account holder's request to move their sign-in to a new address, before the new address has
/// been proven.
/// </summary>
/// <param name="Subject">Whose account. Comes from the credential, never from a request body.</param>
/// <param name="NewEmail">The address to move to.</param>
/// <param name="CurrentPassword">The account's current password, proving the person and not just the session.</param>
public sealed record EmailChangeStart(string Subject, string NewEmail, string CurrentPassword);

namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>The caller's email, local roles and client account permissions, for clients that cannot decode them from the token.</summary>
/// <param name="Email">The account's email address.</param>
/// <param name="Roles">The roles granted locally to the account.</param>
/// <param name="EmailVerified">Whether the credential says the email is confirmed; <c>null</c> when it carries no claim.</param>
/// <param name="ClientPermissions">
/// The <c>ClientPermission</c> flags the caller holds on a client account as an integer: 8191 (all) for the owner of
/// a client record, the stored grant for a linked user, 0 for a caller with no account. It is what the portal reads to
/// hide what the user cannot use; the server enforces the same value on every portal action.
/// </param>
public sealed record MyAccountDto(string Email, IReadOnlyList<string> Roles, bool? EmailVerified, int ClientPermissions);

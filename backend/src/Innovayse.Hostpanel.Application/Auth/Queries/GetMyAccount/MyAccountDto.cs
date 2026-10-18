namespace Innovayse.Hostpanel.Application.Auth.Queries.GetMyAccount;

/// <summary>The caller's email and local roles, for clients that cannot decode roles from the token.</summary>
/// <param name="Email">The account's email address.</param>
/// <param name="Roles">The roles granted locally to the account.</param>
/// <param name="EmailVerified">Whether the credential says the email is confirmed; <c>null</c> when it carries no claim.</param>
public sealed record MyAccountDto(string Email, IReadOnlyList<string> Roles, bool? EmailVerified);

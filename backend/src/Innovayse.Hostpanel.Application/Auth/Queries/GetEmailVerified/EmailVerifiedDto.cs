namespace Innovayse.Hostpanel.Application.Auth.Queries.GetEmailVerified;

/// <summary>Whether the current user's email is confirmed.</summary>
/// <param name="Verified">
/// Three-valued on purpose: <c>null</c> when the credential carries no <c>email_verified</c>
/// claim. Clients must prompt only on an explicit <c>false</c>.
/// </param>
public sealed record EmailVerifiedDto(bool? Verified);

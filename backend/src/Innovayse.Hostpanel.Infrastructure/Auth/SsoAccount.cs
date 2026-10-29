namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>An account as the identity service describes it.</summary>
/// <param name="Subject">The service's identifier for the person.</param>
/// <param name="Email">The address the person signs in with.</param>
/// <param name="FirstName">Given name.</param>
/// <param name="LastName">Family name.</param>
/// <param name="TwoFactorEnabled">Whether a second factor is set up on the account.</param>
public sealed record SsoAccount(
    string Subject, string Email, string FirstName, string LastName, bool TwoFactorEnabled);

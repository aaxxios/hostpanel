namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>The secret and enrolment URI the sign-on provider issued.</summary>
/// <param name="Secret">The shared secret in base32.</param>
/// <param name="QrUri">The <c>otpauth://</c> URI an authenticator app scans.</param>
public sealed record SsoTotpEnrolment(string Secret, string QrUri);

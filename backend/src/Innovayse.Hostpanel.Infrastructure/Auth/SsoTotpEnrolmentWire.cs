using System.Text.Json.Serialization;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>Wire shape of the sign-on provider answer that starts a TOTP enrolment.</summary>
/// <param name="Secret">The shared secret in base32.</param>
/// <param name="QrUri">The <c>otpauth://</c> URI an authenticator app scans.</param>
internal sealed record SsoTotpEnrolmentWire(
    [property: JsonPropertyName("secret")] string Secret,
    [property: JsonPropertyName("qrUri")] string QrUri);

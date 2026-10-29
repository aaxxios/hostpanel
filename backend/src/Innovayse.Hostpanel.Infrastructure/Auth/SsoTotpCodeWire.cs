using System.Text.Json.Serialization;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>Request body carrying a one-time code to the sign-on provider.</summary>
/// <param name="TotpCode">The six-digit code the person typed.</param>
internal sealed record SsoTotpCodeWire(
    [property: JsonPropertyName("totpCode")] string TotpCode);

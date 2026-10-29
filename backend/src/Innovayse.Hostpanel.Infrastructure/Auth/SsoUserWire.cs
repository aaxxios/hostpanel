using System.Text.Json.Serialization;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>Wire shape of one account answered by the sign-on provider user endpoint.</summary>
/// <param name="Email">The address the person signs in with.</param>
/// <param name="FirstName">Given name.</param>
/// <param name="LastName">Family name.</param>
/// <param name="TwoFactorEnabled">Whether a second factor is set up on the account.</param>
internal sealed record SsoUserWire(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("twoFactorEnabled")] bool TwoFactorEnabled);

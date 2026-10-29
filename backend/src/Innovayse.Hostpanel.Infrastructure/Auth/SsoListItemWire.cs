using System.Text.Json.Serialization;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>Wire shape of one account inside a batch or list answer.</summary>
/// <param name="Id">The sign-on provider subject of the account.</param>
/// <param name="Email">The address the person signs in with.</param>
/// <param name="FirstName">Given name.</param>
/// <param name="LastName">Family name.</param>
/// <param name="TwoFactorEnabled">Whether a second factor is set up on the account.</param>
internal sealed record SsoListItemWire(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("twoFactorEnabled")] bool TwoFactorEnabled);

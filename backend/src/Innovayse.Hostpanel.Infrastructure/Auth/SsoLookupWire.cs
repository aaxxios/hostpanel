using System.Text.Json.Serialization;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>Wire shape of the sign-on provider answer to an email lookup.</summary>
/// <param name="UserId">The sign-on provider subject that owns the looked-up address.</param>
internal sealed record SsoLookupWire(
    [property: JsonPropertyName("userId")] string UserId);

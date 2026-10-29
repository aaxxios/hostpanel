using System.Text.Json.Serialization;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>Wire shape of the sign-on provider answer to a batch lookup by subject.</summary>
/// <param name="Users">The accounts the sign-on provider knows among the requested subjects.</param>
internal sealed record SsoBatchWire(
    [property: JsonPropertyName("users")] List<SsoListItemWire> Users);

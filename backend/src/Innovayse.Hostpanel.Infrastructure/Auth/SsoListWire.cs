using System.Text.Json.Serialization;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>Wire shape of one page of the sign-on provider account listing.</summary>
/// <param name="Total">Number of matching accounts before paging.</param>
/// <param name="Users">The accounts on the requested page.</param>
internal sealed record SsoListWire(
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("users")] List<SsoListItemWire> Users);

using System.Text.Json.Serialization;

namespace Innovayse.Providers.CWP7.Models;

/// <summary>The <c>msj</c> object of a CWP7 user-session response.</summary>
internal sealed class Cwp7UserSessionPayload
{
    /// <summary>Gets or initializes how many accounts CWP7 matched.</summary>
    [JsonPropertyName("accounts")]
    public int Accounts { get; init; }

    /// <summary>Gets or initializes one session entry per matched account.</summary>
    [JsonPropertyName("details")]
    public List<Cwp7UserSessionDetail>? Details { get; init; }
}

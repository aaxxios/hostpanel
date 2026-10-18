using System.Text.Json.Serialization;

namespace Innovayse.Providers.CWP7.Models;

/// <summary>One account's freshly minted control-panel session.</summary>
internal sealed class Cwp7UserSessionDetail
{
    /// <summary>Gets or initializes the account username the session belongs to.</summary>
    [JsonPropertyName("user")]
    public string User { get; init; } = string.Empty;

    /// <summary>Gets or initializes the opaque session token.</summary>
    /// <remarks>Carried inside <see cref="Url"/> already; kept because CWP7 sends it.</remarks>
    [JsonPropertyName("token")]
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the ready-to-open control-panel URL with the session token in it.
    /// </summary>
    /// <remarks>
    /// Used as-is rather than rebuilt from <see cref="Token"/>: CWP7 composes it against its own
    /// hostname and port, which need not be the address the API was called on.
    /// </remarks>
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;
}

using System.Text.Json.Serialization;

namespace Innovayse.Providers.CWP.Models;
/// <summary>Parsed response from the CWP REST API.</summary>
internal sealed class CwpApiWire
{
    /// <summary>The <see cref="Status"/> value CWP returns on success.</summary>
    public const string OkStatus = "OK";

    /// <summary>Gets or initializes the result status — "OK" on success, "Error" on failure.</summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets or initializes the human-readable message returned by CWP.</summary>
    [JsonPropertyName("msj")]
    public string Message { get; init; } = string.Empty;

    /// <summary>Returns true when <see cref="Status"/> equals "OK" (case-insensitive).</summary>
    public bool IsSuccess =>
        string.Equals(Status, OkStatus, StringComparison.OrdinalIgnoreCase);
}

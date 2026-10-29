using System.Text.Json;
using System.Text.Json.Serialization;

namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>
/// Reads a client the source plugin sends flat into the grouped <see cref="ClientRecord"/>, so the
/// plugin's output does not change. Records are only ever read, never written back out.
/// </summary>
internal sealed class ClientRecordConverter : JsonConverter<ClientRecord>
{
    /// <inheritdoc/>
    public override ClientRecord? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonSerializer.Deserialize<ClientRecordWire>(ref reader, options)?.ToRecord();

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ClientRecord value, JsonSerializerOptions options) =>
        throw new NotSupportedException("A client record is read from the source and never written back.");
}

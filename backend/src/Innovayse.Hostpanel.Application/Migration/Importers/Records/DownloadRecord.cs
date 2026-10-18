namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One downloadable file as the source plugin describes it.</summary>
/// <param name="Title">The title.</param>
/// <param name="Description">The description.</param>
/// <param name="Type">The type.</param>
/// <param name="Filename">The filename.</param>
/// <param name="CategoryId">The category identifier.</param>
/// <param name="ClientsOnly">Whether clients only.</param>
/// <param name="Hidden">Whether hidden.</param>
internal sealed record DownloadRecord(
    string Title, string? Description, string Type, string Filename,
    int? CategoryId, bool ClientsOnly, bool Hidden);

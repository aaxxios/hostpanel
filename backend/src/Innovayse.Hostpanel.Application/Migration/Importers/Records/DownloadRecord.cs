namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One downloadable file as the source plugin describes it.</summary>
/// <param name="Title">Title shown in the downloads library; when empty the file name is used, then a default title.</param>
/// <param name="Description">Longer descriptive text; null or empty when the source holds none.</param>
/// <param name="Type">The source's file kind (zip, pdf, exe and so on).</param>
/// <param name="Filename">The stored file name in the source.</param>
/// <param name="CategoryId">The source id of the category the record belongs to; null when it has none.</param>
/// <param name="ClientsOnly">Whether only signed-in clients may download it.</param>
/// <param name="Hidden">Whether the source hides it from clients.</param>
internal sealed record DownloadRecord(
    string Title, string? Description, string Type, string Filename,
    int? CategoryId, bool ClientsOnly, bool Hidden);

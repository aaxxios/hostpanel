namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One download category as the source plugin describes it.</summary>
/// <param name="Id">The source's own id, used only to link child records (items, sub-categories) during the import; never stored as a local id.</param>
/// <param name="Name">Category name shown in the downloads library.</param>
/// <param name="Description">Longer descriptive text; null or empty when the source holds none.</param>
/// <param name="Hidden">Whether the source hides it from clients.</param>
/// <param name="ParentId">The source id of the parent category; null for a top-level category.</param>
internal sealed record DownloadCategoryRecord(
    int Id, string Name, string? Description, bool Hidden, int? ParentId);

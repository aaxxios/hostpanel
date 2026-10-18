namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One download category as the source plugin describes it.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Description">The description.</param>
/// <param name="Hidden">Whether hidden.</param>
/// <param name="ParentId">The parent identifier.</param>
internal sealed record DownloadCategoryRecord(
    int Id, string Name, string? Description, bool Hidden, int? ParentId);

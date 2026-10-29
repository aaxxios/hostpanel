namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One product group as the source plugin describes it.</summary>
/// <param name="Id">The source's own id, used only to link child records (items, sub-categories) during the import; never stored as a local id.</param>
/// <param name="Name">Group name shown in the catalog.</param>
/// <param name="Description">Longer descriptive text; null or empty when the source holds none.</param>
internal sealed record ProductGroupRecord(int Id, string Name, string? Description);

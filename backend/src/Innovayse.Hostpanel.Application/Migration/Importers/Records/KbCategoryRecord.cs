namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One knowledge-base category as the source plugin describes it.</summary>
/// <param name="Id">The source's own id, used only to link child records (items, sub-categories) during the import; never stored as a local id.</param>
/// <param name="Name">Category name shown in the knowledge base.</param>
/// <param name="Description">Longer descriptive text; null or empty when the source holds none.</param>
/// <param name="ParentId">The source id of the parent category; null for a top-level category.</param>
internal sealed record KbCategoryRecord(int Id, string Name, string Description, int? ParentId);

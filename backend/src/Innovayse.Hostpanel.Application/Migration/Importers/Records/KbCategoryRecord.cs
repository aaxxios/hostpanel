namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One knowledge-base category as the source plugin describes it.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Description">The description.</param>
/// <param name="ParentId">The parent identifier.</param>
internal sealed record KbCategoryRecord(int Id, string Name, string Description, int? ParentId);

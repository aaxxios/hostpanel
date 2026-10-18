namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One product group as the source plugin describes it.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Description">The description.</param>
internal sealed record ProductGroupRecord(int Id, string Name, string? Description);

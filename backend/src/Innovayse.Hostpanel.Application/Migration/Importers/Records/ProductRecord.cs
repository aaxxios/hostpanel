namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One product as the source plugin describes it.</summary>
/// <param name="Id">The source's own id, used only to link child records (items, sub-categories) during the import; never stored as a local id.</param>
/// <param name="GroupId">The source id of the product group; resolved through the group import map.</param>
/// <param name="Name">Product name shown in the catalog; also the duplicate check.</param>
/// <param name="Description">Longer descriptive text; null or empty when the source holds none.</param>
/// <param name="Type">The source's product type word, mapped to a local product type.</param>
/// <param name="MonthlyPrice">Monthly price in the source's default currency; null when not sold monthly.</param>
/// <param name="AnnualPrice">Annual price in the source's default currency; null when not sold annually.</param>
internal sealed record ProductRecord(int Id, int GroupId, string Name, string? Description, string Type, decimal? MonthlyPrice, decimal? AnnualPrice);

namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One product as the source plugin describes it.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="GroupId">The group identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Description">The description.</param>
/// <param name="Type">The type.</param>
/// <param name="MonthlyPrice">The monthly price.</param>
/// <param name="AnnualPrice">The annual price.</param>
internal sealed record ProductRecord(int Id, int GroupId, string Name, string? Description, string Type, decimal MonthlyPrice, decimal AnnualPrice);

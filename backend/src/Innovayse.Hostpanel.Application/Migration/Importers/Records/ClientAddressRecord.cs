namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>The postal address of one client as the source plugin describes it.</summary>
/// <param name="Street">Street address line, free text.</param>
/// <param name="City">City of the postal address.</param>
/// <param name="State">State or region of the postal address.</param>
/// <param name="PostCode">Postal code of the address.</param>
/// <param name="Country">Two-letter ISO country code of the address.</param>
internal sealed record ClientAddressRecord(string? Street, string? City, string? State, string? PostCode, string? Country);

namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One client contact as the source plugin describes it.</summary>
/// <param name="ClientEmail">Email of the owning client in the source; the importer finds the local client by it and skips the record when none matches.</param>
/// <param name="FirstName">Given name as stored in the source.</param>
/// <param name="LastName">Family name as stored in the source.</param>
/// <param name="Company">Company name; null or empty when the source holds none.</param>
/// <param name="Email">The contact's own email; the duplicate check within the client.</param>
/// <param name="Phone">Phone number, free text as stored in the source.</param>
/// <param name="Address">Street address line, free text.</param>
/// <param name="City">City of the postal address.</param>
/// <param name="State">State or region of the postal address.</param>
/// <param name="PostCode">Postal code of the address.</param>
/// <param name="Country">Two-letter ISO country code of the address.</param>
internal sealed record ContactRecord(
    string ClientEmail, string FirstName, string LastName, string? Company,
    string Email, string? Phone, string? Address, string? City, string? State, string? PostCode, string? Country);

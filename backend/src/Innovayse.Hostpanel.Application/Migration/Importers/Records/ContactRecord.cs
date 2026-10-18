namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One client contact as the source plugin describes it.</summary>
/// <param name="ClientEmail">The client email.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="Company">The company.</param>
/// <param name="Email">The email.</param>
/// <param name="Phone">The phone.</param>
/// <param name="Address">The address.</param>
/// <param name="City">The city.</param>
/// <param name="State">The state.</param>
/// <param name="PostCode">The post code.</param>
/// <param name="Country">The country.</param>
internal sealed record ContactRecord(
    string ClientEmail, string FirstName, string LastName, string? Company,
    string Email, string? Phone, string? Address, string? City, string? State, string? PostCode, string? Country);

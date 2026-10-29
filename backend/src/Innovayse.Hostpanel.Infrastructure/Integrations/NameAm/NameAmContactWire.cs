namespace Innovayse.Hostpanel.Infrastructure.Integrations.NameAm;

/// <summary>One contact block in Name.am's JSON, as it is sent for a role of a domain.</summary>
/// <param name="FirstName">Contact first name.</param>
/// <param name="LastName">Contact last name.</param>
/// <param name="FullName">First and last name joined.</param>
/// <param name="Organization">Organisation, empty when there is none.</param>
/// <param name="Email">Contact e-mail.</param>
/// <param name="Phone">Contact phone.</param>
/// <param name="Address1">Street address.</param>
/// <param name="Address2">Second address line, empty when there is none.</param>
/// <param name="City">City.</param>
/// <param name="State">State or province.</param>
/// <param name="Zip">Postal code.</param>
/// <param name="Country">ISO 3166-1 alpha-2 country code.</param>
internal sealed record NameAmContactWire(
    string FirstName,
    string LastName,
    string FullName,
    string Organization,
    string Email,
    string Phone,
    string Address1,
    string Address2,
    string City,
    string State,
    string Zip,
    string Country);

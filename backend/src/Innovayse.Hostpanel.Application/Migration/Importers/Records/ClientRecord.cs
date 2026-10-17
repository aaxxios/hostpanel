namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One client as the source plugin describes it.</summary>
/// <param name="Email">The email.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="Company">The company.</param>
/// <param name="Phone">The phone.</param>
/// <param name="Address">The address.</param>
/// <param name="City">The city.</param>
/// <param name="State">The state.</param>
/// <param name="PostCode">The post code.</param>
/// <param name="Country">The country.</param>
/// <param name="Status">The status.</param>
/// <param name="Currency">The currency.</param>
/// <param name="TaxExempt">Whether tax exempt.</param>
/// <param name="CreditBalance">The credit balance.</param>
/// <param name="SignupDate">The signup date.</param>
/// <param name="LastLogin">The last login.</param>
internal sealed record ClientRecord(
    string Email, string FirstName, string LastName,
    string? Company, string? Phone,
    string? Address, string? City, string? State, string? PostCode, string? Country,
    string Status, string? Currency, bool TaxExempt, decimal CreditBalance,
    DateTimeOffset? SignupDate, DateTimeOffset? LastLogin);

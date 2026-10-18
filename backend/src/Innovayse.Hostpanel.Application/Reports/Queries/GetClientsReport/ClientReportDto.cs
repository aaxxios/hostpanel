namespace Innovayse.Hostpanel.Application.Reports.Queries.GetClientsReport;

/// <summary>One row in the Clients report.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="CompanyName">The company name.</param>
/// <param name="Email">The email.</param>
/// <param name="Address1">The address1.</param>
/// <param name="Address2">The address2.</param>
/// <param name="City">The city.</param>
/// <param name="State">The state.</param>
/// <param name="PostCode">The post code.</param>
/// <param name="Country">The country.</param>
/// <param name="Phone">The phone.</param>
/// <param name="Currency">The currency.</param>
/// <param name="CreditBalance">The credit balance.</param>
/// <param name="Status">The status.</param>
/// <param name="CreatedAt">The created at.</param>
/// <param name="Notes">The notes.</param>
public record ClientReportDto(
    int Id,
    string FirstName,
    string LastName,
    string? CompanyName,
    string Email,
    string? Address1,
    string? Address2,
    string? City,
    string? State,
    string? PostCode,
    string? Country,
    string? Phone,
    string? Currency,
    decimal CreditBalance,
    string Status,
    string CreatedAt,
    string? Notes);

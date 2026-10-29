using Innovayse.Hostpanel.Domain.Clients.Enums;

namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One row in the Clients report.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="CompanyName">The company name.</param>
/// <param name="Email">The email.</param>
/// <param name="Address">The postal address.</param>
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
    ClientReportAddressDto Address,
    string? Phone,
    string? Currency,
    decimal CreditBalance,
    ClientStatus Status,
    string CreatedAt,
    string? Notes);

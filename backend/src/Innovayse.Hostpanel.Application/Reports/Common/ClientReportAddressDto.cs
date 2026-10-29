namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>The postal address of a client row in the Clients report.</summary>
/// <param name="Address1">The first address line.</param>
/// <param name="Address2">The second address line.</param>
/// <param name="City">The city.</param>
/// <param name="State">The state.</param>
/// <param name="PostCode">The post code.</param>
/// <param name="Country">The country.</param>
public sealed record ClientReportAddressDto(
    string? Address1,
    string? Address2,
    string? City,
    string? State,
    string? PostCode,
    string? Country);

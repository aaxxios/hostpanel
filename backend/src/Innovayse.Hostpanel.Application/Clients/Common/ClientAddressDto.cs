namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>The postal address on a client record, every part optional.</summary>
/// <param name="Street">Street line.</param>
/// <param name="Address2">Second address line.</param>
/// <param name="City">City.</param>
/// <param name="State">State or region.</param>
/// <param name="PostCode">Postal code.</param>
/// <param name="Country">Country as stored on the record.</param>
public sealed record ClientAddressDto(
    string? Street,
    string? Address2,
    string? City,
    string? State,
    string? PostCode,
    string? Country);

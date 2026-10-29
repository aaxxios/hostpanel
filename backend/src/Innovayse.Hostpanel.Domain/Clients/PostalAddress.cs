namespace Innovayse.Hostpanel.Domain.Clients;

/// <summary>A postal address as a <see cref="Contact"/> carries it.</summary>
/// <param name="Street">The street line.</param>
/// <param name="Address2">The second address line.</param>
/// <param name="City">The city.</param>
/// <param name="State">The state or region.</param>
/// <param name="PostCode">The postal code.</param>
/// <param name="Country">The ISO 3166-1 alpha-2 country code; stored upper case.</param>
public sealed record PostalAddress(
    string? Street,
    string? Address2,
    string? City,
    string? State,
    string? PostCode,
    string? Country);

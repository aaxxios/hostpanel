namespace Innovayse.Hostpanel.Application.Locations.Common;

/// <summary>DTO representing a location for the storefront, with the translation flattened for the requested locale.</summary>
/// <param name="Id">Location identifier.</param>
/// <param name="CountryCode">ISO 3166 alpha-2 country code (two letters, any case).</param>
/// <param name="Name">The data-centre name in the resolved locale.</param>
/// <param name="City">The city in the resolved locale, or null.</param>
/// <param name="LatencyNote">The short latency note in the resolved locale, or null.</param>
public sealed record LocationPublicDto(
    int Id,
    string CountryCode,
    string Name,
    string? City,
    string? LatencyNote);

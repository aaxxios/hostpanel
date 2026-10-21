namespace Innovayse.Hostpanel.Application.Locations.Common;

/// <summary>DTO representing one locale-specific translation of a location; the locale is the key it travels under.</summary>
/// <param name="Name">The data-centre name in this locale.</param>
/// <param name="City">The city in this locale, or <c>null</c>.</param>
/// <param name="LatencyNote">A short latency note in this locale, or <c>null</c>.</param>
public sealed record LocationTranslationDto(
    string Name,
    string? City,
    string? LatencyNote);

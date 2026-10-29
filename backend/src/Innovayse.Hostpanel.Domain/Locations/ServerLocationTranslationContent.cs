namespace Innovayse.Hostpanel.Domain.Locations;

/// <summary>The text of one locale of a location, handed to <see cref="ServerLocation.ReplaceTranslations"/>.</summary>
/// <param name="Name">The location's display name in this locale.</param>
/// <param name="City">The city shown under the name; null when the location shows none.</param>
/// <param name="LatencyNote">A short latency hint for visitors; null when the location shows none.</param>
public sealed record ServerLocationTranslationContent(string Name, string? City, string? LatencyNote);

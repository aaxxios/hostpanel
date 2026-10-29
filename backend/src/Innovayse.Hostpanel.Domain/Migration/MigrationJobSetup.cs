namespace Innovayse.Hostpanel.Domain.Migration;

/// <summary>What a new migration job is created from.</summary>
/// <param name="Label">Optional operator-facing label.</param>
/// <param name="SourceUrl">The source system's migration plugin address; a trailing slash is dropped.</param>
/// <param name="Selection">Which entity types the job exports.</param>
public sealed record MigrationJobSetup(string? Label, string SourceUrl, MigrationExportSelection Selection);

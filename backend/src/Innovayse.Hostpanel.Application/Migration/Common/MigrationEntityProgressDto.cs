namespace Innovayse.Hostpanel.Application.Migration.Common;

/// <summary>Progress for a single entity type.</summary>
/// <param name="Imported">Rows written.</param>
/// <param name="Skipped">Rows skipped as duplicates or invalid.</param>
/// <param name="Failed">Rows whose import failed with an error.</param>
/// <param name="Total">How many records of this kind the source reported before the pull started.</param>
/// <param name="Done">Whether this entity type has finished.</param>
public sealed record MigrationEntityProgressDto(
    int Imported,
    int Skipped,
    int Failed,
    int Total,
    bool Done);

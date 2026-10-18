namespace Innovayse.Hostpanel.Application.Migration.Common;

/// <summary>Progress for a single entity type.</summary>
/// <param name="Imported">Rows written.</param>
/// <param name="Skipped">Rows skipped as duplicates or invalid.</param>
/// <param name="Total">The total.</param>
/// <param name="Done">Whether this entity type has finished.</param>
public sealed record MigrationEntityProgressDto(
    int Imported,
    int Skipped,
    int Total,
    bool Done);

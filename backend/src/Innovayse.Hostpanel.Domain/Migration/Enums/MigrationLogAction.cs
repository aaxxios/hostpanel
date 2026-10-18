namespace Innovayse.Hostpanel.Domain.Migration.Enums;
/// <summary>Outcome of a single record processed during a migration job.</summary>
public enum MigrationLogAction
{
    /// <summary>The record was successfully created in the database.</summary>
    Imported = 0,

    /// <summary>The record was intentionally skipped (e.g. duplicate email or domain).</summary>
    Skipped = 1,

    /// <summary>An unexpected error occurred; the record was not imported.</summary>
    Failed = 2,
}

using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Domain.Migration.Enums;

namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>What an importer did with one source record, and why when it skipped it.</summary>
/// <param name="Action">Whether the record was imported or skipped.</param>
/// <param name="SkipReason">Why the record was skipped; null when it was imported.</param>
internal sealed record RecordImportResult(ImportOutcome Action, MigrationSkipReason? SkipReason)
{
    /// <summary>The result of a record that was written here.</summary>
    public static RecordImportResult Imported { get; } = new(ImportOutcome.Imported, null);

    /// <summary>The result of a record whose import threw; the detail is in the server log only.</summary>
    public static RecordImportResult Failed { get; } = new(ImportOutcome.Failed, MigrationSkipReason.ImportFailed);

    /// <summary>Creates the result of a record that was left alone.</summary>
    /// <param name="reason">Why the record was skipped.</param>
    /// <returns>A skipped result carrying <paramref name="reason"/>.</returns>
    public static RecordImportResult Skipped(MigrationSkipReason reason) => new(ImportOutcome.Skipped, reason);
}

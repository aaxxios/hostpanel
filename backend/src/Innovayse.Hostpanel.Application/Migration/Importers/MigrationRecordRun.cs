using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;

namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>One dataset an importer hands to <see cref="MigrationRecordRunner"/>: where to read it and how to import one record of it.</summary>
/// <typeparam name="TRecord">The record shape this dataset's pages are read into.</typeparam>
/// <param name="Job">The migration job being run, already started with its totals recorded.</param>
/// <param name="EntityType">Which kind of entity the records are, for the job's counters and log.</param>
/// <param name="DataSet">Which dataset to page through, in the source plugin's own vocabulary.</param>
/// <param name="ImportRecordAsync">Imports one record and says whether it was imported or skipped, and why.</param>
internal sealed record MigrationRecordRun<TRecord>(
    MigrationJob Job,
    MigrationEntityType EntityType,
    string DataSet,
    Func<TRecord, CancellationToken, Task<RecordImportResult>> ImportRecordAsync);

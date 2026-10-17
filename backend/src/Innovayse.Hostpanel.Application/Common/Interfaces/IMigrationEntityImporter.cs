using Innovayse.Hostpanel.Domain.Migration;

namespace Innovayse.Hostpanel.Application.Common.Interfaces;

/// <summary>
/// One imported entity type's page-by-page pull from the migration source plugin into the
/// Innovayse database. <see cref="Migration.Pull.MigrationPullWorker"/> is the orchestrator that runs one of
/// these per entity kind a job selected, in the same dependency order it always ran in.
/// </summary>
public interface IMigrationEntityImporter
{
    /// <summary>
    /// Pulls every page of this importer's entity kind for <paramref name="job"/>, imports each
    /// record, writes one <see cref="Domain.Migration.MigrationLog"/> row per record, and updates
    /// the job's own progress counters for this entity kind after each page.
    /// </summary>
    /// <param name="job">The migration job being run, already started with its totals recorded.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once every page for this entity kind has been imported.</returns>
    Task ImportAsync(MigrationJob job, CancellationToken ct);
}

namespace Innovayse.Hostpanel.Application.Migration.Commands.RunMigrationPull;

/// <summary>
/// Runs the pull of one queued migration job on the background message queue. Published only by
/// <see cref="Pull.MigrationPullQueue"/>, after the job's queued state was saved.
/// </summary>
/// <param name="Id">The queued migration job to pull.</param>
public sealed record RunMigrationPullCommand(int Id);

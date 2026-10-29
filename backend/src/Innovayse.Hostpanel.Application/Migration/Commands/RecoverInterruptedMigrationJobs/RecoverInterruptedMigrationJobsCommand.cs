using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Migration.Commands.RecoverInterruptedMigrationJobs;

/// <summary>
/// Fails every migration job that is queued or in progress but has no worker behind it any more (its
/// heartbeat is stale), so an admin can retry it. A recurring job: scheduled on startup by
/// <c>MigrationRecoveryStartup</c> and by its own handler after every pass.
/// </summary>
public sealed record RecoverInterruptedMigrationJobsCommand : IRecurringJob;

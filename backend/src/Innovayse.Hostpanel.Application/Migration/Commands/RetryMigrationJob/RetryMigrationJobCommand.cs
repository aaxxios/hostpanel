namespace Innovayse.Hostpanel.Application.Migration.Commands.RetryMigrationJob;

/// <summary>Queues a failed migration job's pull again.</summary>
/// <param name="Id">The failed migration job to retry; overlaid from the route.</param>
public sealed record RetryMigrationJobCommand(int Id);

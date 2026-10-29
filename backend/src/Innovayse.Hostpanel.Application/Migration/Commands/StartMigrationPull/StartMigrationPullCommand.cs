namespace Innovayse.Hostpanel.Application.Migration.Commands.StartMigrationPull;

/// <summary>Triggers the panel to begin pulling data from the migration plugin.</summary>
/// <param name="Id">The migration job to start pulling; overlaid from the route.</param>
public sealed record StartMigrationPullCommand(int Id);

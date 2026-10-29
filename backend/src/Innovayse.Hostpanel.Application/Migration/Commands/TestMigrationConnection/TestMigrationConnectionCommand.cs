namespace Innovayse.Hostpanel.Application.Migration.Commands.TestMigrationConnection;

/// <summary>Tests the connection from the panel to the migration plugin at SourceUrl.</summary>
/// <param name="Id">The migration job whose source address and key are tested; overlaid from the route.</param>
public sealed record TestMigrationConnectionCommand(int Id);

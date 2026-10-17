using Innovayse.Hostpanel.Application.Migration.Common;

namespace Innovayse.Hostpanel.Application.Migration.Commands.CreateMigrationJob;

/// <summary>Creates a new pending migration job and returns its ID and secret key.</summary>
/// <param name="Label">Optional operator-facing label for the job.</param>
/// <param name="SourceUrl">The address of the source system's migration plugin.</param>
/// <param name="Selection">Which entity types to export from the source.</param>
public sealed record CreateMigrationJobCommand(
    string? Label,
    string SourceUrl,
    MigrationEntitySelectionDto Selection);

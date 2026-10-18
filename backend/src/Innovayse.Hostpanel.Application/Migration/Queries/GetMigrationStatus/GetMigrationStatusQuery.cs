namespace Innovayse.Hostpanel.Application.Migration.Queries.GetMigrationStatus;

/// <summary>Returns the current status and progress of a migration job.</summary>
/// <param name="JobId">The job identifier.</param>
public sealed record GetMigrationStatusQuery(int JobId);

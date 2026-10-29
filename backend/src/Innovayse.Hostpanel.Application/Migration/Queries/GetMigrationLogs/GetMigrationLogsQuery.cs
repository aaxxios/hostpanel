using Innovayse.Hostpanel.Domain.Migration.Enums;

namespace Innovayse.Hostpanel.Application.Migration.Queries.GetMigrationLogs;

/// <summary>Returns a paginated list of log entries for a migration job.</summary>
/// <param name="JobId">The job whose report to read; overlaid from the route.</param>
/// <param name="Action">Optional filter: only entries with this outcome.</param>
/// <param name="EntityType">Optional filter: only entries for this entity type.</param>
/// <param name="Page">Which page to return, counting from 1. Defaults match the controller's
/// former parameter defaults, since a bound record has no method signature to carry them on.</param>
/// <param name="PageSize">How many entries a page holds at most.</param>
public sealed record GetMigrationLogsQuery(
    int JobId,
    MigrationLogAction? Action,
    MigrationEntityType? EntityType,
    int Page = 1,
    int PageSize = 50);

using Innovayse.Hostpanel.Application.Migration.Common;

namespace Innovayse.Hostpanel.Application.Migration.Queries.GetMigrationLogs;

/// <summary>Paged result of migration log entries.</summary>
/// <param name="Items">The items entries.</param>
/// <param name="TotalCount">The total count.</param>
/// <param name="Page">The page.</param>
/// <param name="PageSize">The page size.</param>
public sealed record MigrationLogPageDto(
    IReadOnlyList<MigrationLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

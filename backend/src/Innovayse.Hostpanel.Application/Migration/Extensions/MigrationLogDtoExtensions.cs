using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Domain.Migration;

namespace Innovayse.Hostpanel.Application.Migration.Extensions;

/// <summary>Extension methods for mapping <see cref="MigrationLog"/> to DTOs.</summary>
public static class MigrationLogDtoExtensions
{
    /// <summary>Maps a <see cref="MigrationLog"/> to a <see cref="MigrationLogDto"/>.</summary>
    /// <param name="log">The log entry to map.</param>
    /// <returns>The DTO describing the entry.</returns>
    public static MigrationLogDto ToDto(this MigrationLog log) =>
        new(log.Id, log.EntityType, log.Identifier, log.Action, log.SkipReason, log.Reason, log.CreatedAt);
}

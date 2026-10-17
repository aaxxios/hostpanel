using Innovayse.Hostpanel.Domain.Migration;

namespace Innovayse.Hostpanel.Application.Migration.Common;
/// <summary>Single migration log entry returned to the admin frontend.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="EntityType">The entity type.</param>
/// <param name="Identifier">The identifier.</param>
/// <param name="Action">The action.</param>
/// <param name="Reason">The reason.</param>
/// <param name="CreatedAt">The created at.</param>
public sealed record MigrationLogDto(
    int Id,
    string EntityType,
    string Identifier,
    string Action,
    string? Reason,
    DateTimeOffset CreatedAt);

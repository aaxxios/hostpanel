using Innovayse.Hostpanel.Domain.Migration;

namespace Innovayse.Hostpanel.Application.Migration.Common;
/// <summary>Migration job summary returned to the admin frontend.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="Key">The key.</param>
/// <param name="SourceUrl">The source url.</param>
/// <param name="Status">The status.</param>
/// <param name="Label">The label.</param>
/// <param name="ErrorMessage">The error message.</param>
/// <param name="EntitySelection">The entity selection.</param>
/// <param name="Progress">The progress.</param>
/// <param name="OverallPercent">The overall percent.</param>
/// <param name="PluginConnected">Whether plugin connected.</param>
/// <param name="LastPingAt">The last ping at.</param>
/// <param name="CreatedAt">The created at.</param>
/// <param name="CompletedAt">The completed at.</param>
public sealed record MigrationJobDto(
    int Id,
    string Key,
    string SourceUrl,
    string Status,
    string? Label,
    string? ErrorMessage,
    MigrationEntitySelectionDto EntitySelection,
    MigrationProgressDto Progress,
    int OverallPercent,
    bool PluginConnected,
    DateTimeOffset? LastPingAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

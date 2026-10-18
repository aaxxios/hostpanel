using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Admin.Servers.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Queries.ListServerGroups;

/// <summary>
/// Handles <see cref="ListServerGroupsQuery"/> by loading all groups with their servers.
/// </summary>
public sealed class ListServerGroupsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListServerGroupsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Loads all server groups and maps them to <see cref="ServerGroupDto"/>.
    /// </summary>
    /// <param name="query">The list query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every configured group, always succeeding.</returns>
    public async Task<Result<IReadOnlyList<ServerGroupDto>>> HandleAsync(
        ListServerGroupsQuery query, CancellationToken ct)
    {
        var groups = await _context.ServerGroups
            .AsNoTracking()
            .Include(g => g.Servers)
            .ToListAsync(ct);

        IReadOnlyList<ServerGroupDto> dtos = groups.Select(ToDto).ToList();
        return Result<IReadOnlyList<ServerGroupDto>>.Success(dtos);
    }

    /// <summary>
    /// Maps a <see cref="ServerGroup"/> to a <see cref="ServerGroupDto"/>.
    /// </summary>
    /// <param name="g">The server group entity.</param>
    /// <returns>Mapped DTO.</returns>
    private static ServerGroupDto ToDto(ServerGroup g)
    {
        var groupMap = new Dictionary<int, string> { [g.Id] = g.Name };
        return new ServerGroupDto(
            g.Id, g.Name, g.FillType.ToString(),
            g.Servers.Select(s => ServerMapper.ToDto(s, groupMap)).ToList(),
            g.CreatedAt);
    }
}

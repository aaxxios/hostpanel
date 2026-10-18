using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Admin.Servers.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Queries.ListServers;

/// <summary>
/// Handles <see cref="ListServersQuery"/> by loading all servers from the database.
/// </summary>
public sealed class ListServersHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListServersHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Loads servers and maps them to <see cref="ServerDto"/>.
    /// </summary>
    /// <param name="query">The list query with optional module filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every matching server, always succeeding.</returns>
    public async Task<Result<IReadOnlyList<ServerDto>>> HandleAsync(ListServersQuery query, CancellationToken ct)
    {
        var servers = await _context.Servers
            .AsNoTracking()
            .Where(s => query.Module == null || s.Module == query.Module)
            .ToListAsync(ct);

        var groups = await _context.ServerGroups.AsNoTracking().ToListAsync(ct);
        var groupMap = groups.ToDictionary(g => g.Id, g => g.Name);

        IReadOnlyList<ServerDto> dtos = servers.Select(s => ServerMapper.ToDto(s, groupMap)).ToList();
        return Result<IReadOnlyList<ServerDto>>.Success(dtos);
    }
}

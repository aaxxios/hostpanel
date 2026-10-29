using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Admin.Servers.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Queries.GetServerSshTarget;

/// <summary>Handles <see cref="GetServerSshTargetQuery"/>: the server and its SSH settings, read without tracking.</summary>
public sealed class GetServerSshTargetHandler
{
    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initialises the handler.</summary>
    /// <param name="context">Data context.</param>
    public GetServerSshTargetHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">Names the server whose SSH target is read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The target, or <c>SERVER_NOT_FOUND</c>.</returns>
    public async Task<Result<ServerSshTargetDto>> HandleAsync(GetServerSshTargetQuery query, CancellationToken ct)
    {
        var server = await _context.Servers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == query.ServerId, ct);
        if (server is null)
        {
            return Result<ServerSshTargetDto>.FailureCode(HostpanelErrorCodes.SERVER_NOT_FOUND);
        }

        var settings = await _context.ServerSshSettings.AsNoTracking().FirstOrDefaultAsync(s => s.ServerId == server.Id, ct);
        return Result<ServerSshTargetDto>.Success(server.ToSshTarget(settings));
    }
}

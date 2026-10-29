using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Admin.Servers.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Queries.GetServerConnection;

/// <summary>Handles <see cref="GetServerConnectionQuery"/>: the server's connection details, read without tracking.</summary>
public sealed class GetServerConnectionHandler
{
    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initialises the handler.</summary>
    /// <param name="context">Data context.</param>
    public GetServerConnectionHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">Names the server whose connection details are read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The connection details, or <c>SERVER_NOT_FOUND</c>.</returns>
    public async Task<Result<ServerConnectionDto>> HandleAsync(GetServerConnectionQuery query, CancellationToken ct)
    {
        var server = await _context.Servers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == query.ServerId, ct);
        if (server is null)
        {
            return Result<ServerConnectionDto>.FailureCode(HostpanelErrorCodes.SERVER_NOT_FOUND);
        }

        return Result<ServerConnectionDto>.Success(server.ToConnection());
    }
}

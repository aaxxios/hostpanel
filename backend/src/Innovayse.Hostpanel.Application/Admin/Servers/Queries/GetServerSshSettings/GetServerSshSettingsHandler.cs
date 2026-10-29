using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Admin.Servers.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Queries.GetServerSshSettings;

/// <summary>Handles <see cref="GetServerSshSettingsQuery"/>; an unconfigured server answers disabled defaults.</summary>
public sealed class GetServerSshSettingsHandler
{
    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Provider factory, to report support.</summary>
    private readonly ISshKeyProviderFactory _factory;

    /// <summary>Initialises the handler.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="factory">Provider factory, to report support.</param>
    public GetServerSshSettingsHandler(IHostpanelDbContext context, ISshKeyProviderFactory factory)
    {
        _context = context;
        _factory = factory;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">Names the server whose SSH settings are read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The settings, or <c>SERVER_NOT_FOUND</c>.</returns>
    public async Task<Result<ServerSshSettingsDto>> HandleAsync(GetServerSshSettingsQuery query, CancellationToken ct)
    {
        var server = await _context.Servers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == query.ServerId, ct);
        if (server is null)
        {
            return Result<ServerSshSettingsDto>.FailureCode(HostpanelErrorCodes.SERVER_NOT_FOUND);
        }

        var settings = await _context.ServerSshSettings.AsNoTracking().FirstOrDefaultAsync(s => s.ServerId == server.Id, ct)
            ?? ServerSshSettings.CreateFor(server.Id);
        return Result<ServerSshSettingsDto>.Success(settings.ToDto(_factory.Supports(server.Module)));
    }
}

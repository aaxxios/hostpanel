using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Admin.Servers.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.ConfigureServerSsh;

/// <summary>Handles <see cref="ConfigureServerSshCommand"/>.</summary>
public sealed class ConfigureServerSshHandler
{
    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Provider factory, to refuse enabling an unsupported module.</summary>
    private readonly ISshKeyProviderFactory _factory;

    /// <summary>Initialises the handler.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="factory">Provider factory, to refuse enabling an unsupported module.</param>
    public ConfigureServerSshHandler(IHostpanelDbContext context, ISshKeyProviderFactory factory)
    {
        _context = context;
        _factory = factory;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The saved settings, or <c>SERVER_NOT_FOUND</c>, <c>SSH_MODULE_NOT_SUPPORTED</c>,
    /// <c>SSH_SETTINGS_INCOMPLETE</c>.
    /// </returns>
    public async Task<Result<ServerSshSettingsDto>> HandleAsync(ConfigureServerSshCommand cmd, CancellationToken ct)
    {
        var server = await _context.Servers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.ServerId, ct);
        if (server is null)
        {
            return Result<ServerSshSettingsDto>.FailureCode(HostpanelErrorCodes.SERVER_NOT_FOUND);
        }

        var supported = _factory.Supports(server.Module);
        if (cmd.IsEnabled && !supported)
        {
            return Result<ServerSshSettingsDto>.FailureCode(HostpanelErrorCodes.SSH_MODULE_NOT_SUPPORTED);
        }

        var settings = await _context.ServerSshSettings.FirstOrDefaultAsync(s => s.ServerId == server.Id, ct);
        if (settings is null)
        {
            settings = ServerSshSettings.CreateFor(server.Id);
            _context.ServerSshSettings.Add(settings);
        }

        try
        {
            settings.Configure(
                cmd.IsEnabled, cmd.SshHostname, cmd.SshPort, cmd.AgentHost, cmd.AgentPort, cmd.AgentUsername,
                string.IsNullOrWhiteSpace(cmd.AgentPrivateKey) ? null : cmd.AgentPrivateKey,
                cmd.AgentHostKeyFingerprint, cmd.ConnectTimeoutSeconds, cmd.OperationTimeoutSeconds);
        }
        catch (ArgumentException)
        {
            // The aggregate refuses an enabled configuration that lacks a value an agent call needs.
            return Result<ServerSshSettingsDto>.FailureCode(HostpanelErrorCodes.SSH_SETTINGS_INCOMPLETE);
        }

        await _context.SaveChangesAsync(ct);
        return Result<ServerSshSettingsDto>.Success(settings.ToDto(supported));
    }
}

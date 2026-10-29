using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Admin.Servers.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.ConfigureServerSsh;

/// <summary>Handles <see cref="ConfigureServerSshCommand"/>.</summary>
public sealed class ConfigureServerSshHandler
{
    /// <summary>The administrator, for the audit trail.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Logger; also the admin audit trail.</summary>
    private readonly ILogger<ConfigureServerSshHandler> _logger;

    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Provider factory, to refuse enabling an unsupported module.</summary>
    private readonly ISshKeyProviderFactory _factory;

    /// <summary>Initialises the handler.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="factory">Provider factory, to refuse enabling an unsupported module.</param>
    /// <param name="caller">The administrator, for the audit trail.</param>
    /// <param name="logger">Logger; also the admin audit trail.</param>
    public ConfigureServerSshHandler(IHostpanelDbContext context, ISshKeyProviderFactory factory, ICurrentUserService caller, ILogger<ConfigureServerSshHandler> logger)
    {
        _caller = caller;
        _logger = logger;
        _context = context;
        _factory = factory;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The saved settings, or <c>SERVER_NOT_FOUND</c>, <c>SSH_MODULE_NOT_SUPPORTED</c>,
    /// <c>SSH_SETTINGS_INCOMPLETE</c>, <c>SSH_AGENT_NOT_VERIFIED</c>, <c>SSH_AGENT_ENDPOINT_IN_USE</c>.
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

        if (cmd.IsEnabled)
        {
            // Customers are only ever pointed at an agent HostPanel has verified, with these exact
            // settings: any change to the endpoint, user, key or pin clears the last health check.
            if (settings.IsHealthy != true)
            {
                return Result<ServerSshSettingsDto>.FailureCode(HostpanelErrorCodes.SSH_AGENT_NOT_VERIFIED);
            }

            // One agent manages one server record. Two records on the same endpoint would let keys
            // meant for one server's accounts land on the other's.
            var host = settings.AgentHost.ToLowerInvariant();
            var port = settings.AgentPort;
            var taken = await _context.ServerSshSettings.AsNoTracking()
                .AnyAsync(s => s.ServerId != server.Id && s.IsEnabled && s.AgentHost.ToLower() == host && s.AgentPort == port, ct);
            if (taken)
            {
                return Result<ServerSshSettingsDto>.FailureCode(HostpanelErrorCodes.SSH_AGENT_ENDPOINT_IN_USE);
            }
        }

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation(
            "SSH settings of server {ServerId} saved by user {UserId}: enabled={Enabled}, key replaced={KeyReplaced}",
            server.Id, _caller.UserId, settings.IsEnabled, !string.IsNullOrWhiteSpace(cmd.AgentPrivateKey));
        return Result<ServerSshSettingsDto>.Success(settings.ToDto(supported));
    }
}

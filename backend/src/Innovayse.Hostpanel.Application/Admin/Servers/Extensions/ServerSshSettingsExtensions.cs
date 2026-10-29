using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Domain.Servers;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Extensions;

/// <summary>Mapping helpers for <see cref="ServerSshSettings"/> and the server it belongs to.</summary>
public static class ServerSshSettingsExtensions
{
    /// <summary>Maps settings to the admin DTO, withholding the private key.</summary>
    /// <param name="settings">The server's stored SSH settings, private key included.</param>
    /// <param name="isSupported">Whether the server's module is supported.</param>
    /// <returns>The DTO.</returns>
    public static ServerSshSettingsDto ToDto(this ServerSshSettings settings, bool isSupported) =>
        new(settings.ServerId, isSupported, settings.IsEnabled, settings.SshHostname, settings.SshPort,
            settings.AgentHost, settings.AgentPort, settings.AgentUsername,
            !string.IsNullOrWhiteSpace(settings.AgentPrivateKey), settings.AgentHostKeyFingerprint,
            settings.ConnectTimeoutSeconds, settings.OperationTimeoutSeconds, settings.IsHealthy, settings.LastHealthCheckAt);

    /// <summary>Maps a server and its SSH settings to what an SSH key provider needs, private key included.</summary>
    /// <param name="server">The server the settings belong to.</param>
    /// <param name="settings">Its SSH settings, when stored.</param>
    /// <returns>The server-side target.</returns>
    public static ServerSshTargetDto ToSshTarget(this Server server, ServerSshSettings? settings) =>
        new(server.Id, server.Module, server.IsDisabled, settings is null
            ? null
            : new ServerSshAgentEndpoint(settings.IsEnabled, settings.IsComplete, settings.SshHostname, settings.SshPort,
                settings.AgentHost, settings.AgentPort, settings.AgentUsername, settings.AgentPrivateKey,
                settings.AgentHostKeyFingerprint, settings.ConnectTimeoutSeconds, settings.OperationTimeoutSeconds));
}

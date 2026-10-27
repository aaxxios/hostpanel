using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Domain.Ssh;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Extensions;

/// <summary>Mapping helpers for <see cref="ServerSshSettings"/>.</summary>
public static class ServerSshSettingsExtensions
{
    /// <summary>Maps settings to the admin DTO, withholding the private key.</summary>
    /// <param name="settings">The settings.</param>
    /// <param name="isSupported">Whether the server's module is supported.</param>
    /// <returns>The DTO.</returns>
    public static ServerSshSettingsDto ToDto(this ServerSshSettings settings, bool isSupported) =>
        new(settings.ServerId, isSupported, settings.IsEnabled, settings.SshHostname, settings.SshPort,
            settings.AgentHost, settings.AgentPort, settings.AgentUsername,
            !string.IsNullOrWhiteSpace(settings.AgentPrivateKey), settings.AgentHostKeyFingerprint,
            settings.ConnectTimeoutSeconds, settings.OperationTimeoutSeconds, settings.IsHealthy, settings.LastHealthCheckAt);
}

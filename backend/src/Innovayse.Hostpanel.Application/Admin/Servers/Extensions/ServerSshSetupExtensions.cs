using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Ssh;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Extensions;

/// <summary>Builds <see cref="ServerSshSetupDto"/>.</summary>
public static class ServerSshSetupExtensions
{
    /// <summary>Path the anonymous bundle endpoint is served on.</summary>
    public const string BundlePath = "/api/public/ssh-agent/bundle.tar.gz";

    /// <summary>Default management user, as the installer names it.</summary>
    private const string DefaultAgentUsername = "hpsshkeys";

    /// <summary>Maps settings and the shipped bundle to the setup DTO.</summary>
    /// <param name="settings">The server's settings, or null when none exist yet.</param>
    /// <param name="serverId">The server.</param>
    /// <param name="profile">The agent profile for its module.</param>
    /// <param name="bundle">The bundle this build ships.</param>
    /// <returns>The DTO.</returns>
    public static ServerSshSetupDto ToSetupDto(this ServerSshSettings? settings, int serverId, string? profile, ISshAgentBundle bundle) =>
        new(serverId, profile, settings?.AgentPublicKey, settings?.AgentBinding,
            string.IsNullOrWhiteSpace(settings?.AgentUsername) ? DefaultAgentUsername : settings.AgentUsername,
            bundle.Version, bundle.Sha256, BundlePath);
}

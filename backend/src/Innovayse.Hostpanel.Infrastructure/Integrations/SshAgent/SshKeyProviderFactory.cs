using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Domain.Ssh;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.SshAgent;

/// <summary>
/// Default <see cref="ISshKeyProviderFactory"/>. Servers of the v7 hosting panel module are served
/// by the agent-backed provider; every other module answers unsupported until it gets an
/// implementation of its own.
/// </summary>
public sealed class SshKeyProviderFactory : ISshKeyProviderFactory
{
    /// <summary>The agent transport.</summary>
    private readonly ISshAgentTransport _transport;

    /// <summary>Initialises the factory.</summary>
    /// <param name="transport">The agent transport.</param>
    public SshKeyProviderFactory(ISshAgentTransport transport)
    {
        _transport = transport;
    }

    /// <inheritdoc/>
    public bool Supports(ServerModule module) => module is ServerModule.Cwp7;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown when the settings belong to another server or are incomplete.</exception>
    public ISshKeyProvider? CreateFor(Server server, ServerSshSettings settings)
    {
        if (!Supports(server.Module))
        {
            return null;
        }

        if (settings.ServerId != server.Id)
        {
            throw new InvalidOperationException($"SSH settings of server {settings.ServerId} were offered for server {server.Id}.");
        }

        if (!settings.IsComplete)
        {
            throw new InvalidOperationException($"SSH settings of server {server.Id} are incomplete.");
        }

        var endpoint = new SshAgentEndpoint(
            server.Id,
            settings.AgentHost,
            settings.AgentPort,
            settings.AgentUsername,
            settings.AgentPrivateKey!,
            settings.AgentHostKeyFingerprint,
            TimeSpan.FromSeconds(settings.ConnectTimeoutSeconds),
            TimeSpan.FromSeconds(settings.OperationTimeoutSeconds));

        return new AgentSshKeyProvider(endpoint, _transport);
    }
}

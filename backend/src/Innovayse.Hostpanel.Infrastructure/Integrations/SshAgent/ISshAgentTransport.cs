namespace Innovayse.Hostpanel.Infrastructure.Integrations.SshAgent;

/// <summary>
/// Carries one JSON request to a server's key agent and returns its one JSON response. The seam
/// that lets <see cref="AgentSshKeyProvider"/> be tested without a server.
/// </summary>
public interface ISshAgentTransport
{
    /// <summary>Sends a request and reads the response.</summary>
    /// <param name="endpoint">The server's agent endpoint.</param>
    /// <param name="requestJson">The request document.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The response document.</returns>
    /// <exception cref="Innovayse.Hostpanel.Domain.Ssh.SshProviderException">
    /// Thrown with <see cref="Innovayse.Hostpanel.Domain.Ssh.SshProviderException.Unreachable"/> on any
    /// connection, authentication, host-key or timeout failure.
    /// </exception>
    public Task<string> SendAsync(SshAgentEndpoint endpoint, string requestJson, CancellationToken ct);
}

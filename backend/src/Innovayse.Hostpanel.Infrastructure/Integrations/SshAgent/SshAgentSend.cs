using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.SshAgent;

/// <summary>
/// Carries one JSON request to a server's key agent and returns its one JSON response.
/// <see cref="SshAgentTransport.SendAsync"/> in production; a recording stand-in in the adapter tests.
/// </summary>
/// <param name="endpoint">The server's agent endpoint.</param>
/// <param name="requestJson">The request document.</param>
/// <param name="ct">Cancellation token.</param>
/// <returns>The response document.</returns>
/// <exception cref="SshProviderException">
/// Thrown with <see cref="SshProviderFailure.Unreachable"/> on any connection, authentication,
/// host-key or timeout failure.
/// </exception>
internal delegate Task<string> SshAgentSend(SshAgentEndpoint endpoint, string requestJson, CancellationToken ct);

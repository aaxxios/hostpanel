using System.Text;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.Infrastructure.Security;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.SshAgent;

/// <summary>
/// Carries one JSON request to a server's key agent over SSH and returns its one JSON response.
/// <see cref="AgentSshKeyProvider"/> calls it through <see cref="SshAgentSend"/>, the seam that lets the
/// provider be tested without a server.
/// </summary>
/// <remarks>
/// The management user's only authorized key is pinned to a forced command on the server, so the
/// command text sent here is irrelevant to what runs: it is left empty, and the request travels
/// on stdin. The server's host key is checked against the pinned fingerprint before
/// authentication; a mismatch aborts the connection, so a spoofed host never receives a request.
/// A fresh connection is opened per call: key operations are rare, and no long-lived session can
/// outlive a settings change.
/// </remarks>
internal sealed class SshAgentTransport
{
    /// <summary>Largest response accepted from an agent.</summary>
    private const int MaxResponseBytes = 64 * 1024;

    /// <summary>Resolves the agent host and refuses loopback, link-local and metadata addresses.</summary>
    private readonly OutboundConnector _connector;

    /// <summary>Logger.</summary>
    private readonly ILogger<SshAgentTransport> _logger;

    /// <summary>Initialises the transport.</summary>
    /// <param name="connector">Resolves the agent host and refuses loopback, link-local and metadata addresses.</param>
    /// <param name="logger">Records why an agent call failed, detail the caller never sees.</param>
    public SshAgentTransport(OutboundConnector connector, ILogger<SshAgentTransport> logger)
    {
        _connector = connector;
        _logger = logger;
    }

    /// <summary>Sends a request and reads the response.</summary>
    /// <param name="endpoint">The server's agent endpoint.</param>
    /// <param name="requestJson">The request document.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The response document.</returns>
    /// <exception cref="SshProviderException">
    /// Thrown with <see cref="SshProviderFailure.Unreachable"/> on any connection, authentication,
    /// host-key or timeout failure, and with <see cref="SshProviderFailure.InternalError"/> for an
    /// oversized response.
    /// </exception>
    public async Task<string> SendAsync(SshAgentEndpoint endpoint, string requestJson, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(endpoint.ConnectTimeout + endpoint.OperationTimeout);

        try
        {
            // The agent host is admin input: it is resolved here and the vetted address is what
            // SSH.NET dials, so the name cannot re-resolve to a refused address in between. The pinned
            // host key still decides whether the far end is the agent.
            var address = (await _connector.ResolveAsync(endpoint.Host, timeout.Token))[0];

            using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(endpoint.PrivateKey));
            using var keyFile = new PrivateKeyFile(keyStream);
            var connection = new ConnectionInfo(
                address.ToString(), endpoint.Port, endpoint.Username, new PrivateKeyAuthenticationMethod(endpoint.Username, keyFile))
            {
                Timeout = endpoint.ConnectTimeout,
            };

            using var client = new SshClient(connection);
            var pinned = endpoint.HostKeyFingerprint.StartsWith("SHA256:", StringComparison.Ordinal)
                ? endpoint.HostKeyFingerprint["SHA256:".Length..]
                : endpoint.HostKeyFingerprint;
            client.HostKeyReceived += (_, e) => e.CanTrust = string.Equals(e.FingerPrintSHA256, pinned, StringComparison.Ordinal);

            await client.ConnectAsync(timeout.Token);
            using var command = client.CreateCommand(string.Empty);
            command.CommandTimeout = endpoint.OperationTimeout;

            var execution = command.ExecuteAsync(timeout.Token);
            using (var input = command.CreateInputStream())
            {
                await input.WriteAsync(Encoding.UTF8.GetBytes(requestJson), timeout.Token);
            }

            await execution;
            client.Disconnect();

            var result = command.Result;
            if (Encoding.UTF8.GetByteCount(result) > MaxResponseBytes)
            {
                throw new SshProviderException(SshProviderFailure.InternalError, $"Agent response on server {endpoint.ServerId} too large.");
            }

            return result;
        }
        catch (SshProviderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is SshException or IOException or System.Net.Sockets.SocketException
                                       or OperationCanceledException or TimeoutException)
        {
            if (ct.IsCancellationRequested)
            {
                throw;
            }

            // Host and port go to the log only; the exception message never reaches a customer.
            _logger.LogWarning(ex, "SSH agent on server {ServerId} ({Host}:{Port}) unreachable.",
                endpoint.ServerId, endpoint.Host, endpoint.Port);
            throw new SshProviderException(SshProviderFailure.Unreachable, $"SSH agent on server {endpoint.ServerId} unreachable.");
        }
    }
}

using System.Text;
using Innovayse.Hostpanel.Domain.Ssh;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.SshAgent;

/// <summary>
/// <see cref="ISshAgentTransport"/> over SSH, using SSH.NET.
/// </summary>
/// <remarks>
/// The management user's only authorized key is pinned to a forced command on the server, so the
/// command text sent here is irrelevant to what runs — it is left empty, and the request travels
/// on stdin. The server's host key is checked against the pinned fingerprint before
/// authentication; a mismatch aborts the connection, so a spoofed host never receives a request.
/// A fresh connection is opened per call: key operations are rare, and no long-lived session can
/// outlive a settings change.
/// </remarks>
public sealed class SshNetAgentTransport : ISshAgentTransport
{
    /// <summary>Largest response accepted from an agent.</summary>
    private const int MaxResponseBytes = 64 * 1024;

    /// <summary>Logger.</summary>
    private readonly ILogger<SshNetAgentTransport> _logger;

    /// <summary>Initialises the transport.</summary>
    /// <param name="logger">Logger.</param>
    public SshNetAgentTransport(ILogger<SshNetAgentTransport> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> SendAsync(SshAgentEndpoint endpoint, string requestJson, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(endpoint.ConnectTimeout + endpoint.OperationTimeout);

        try
        {
            using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(endpoint.PrivateKey));
            using var keyFile = new PrivateKeyFile(keyStream);
            var connection = new ConnectionInfo(
                endpoint.Host, endpoint.Port, endpoint.Username, new PrivateKeyAuthenticationMethod(endpoint.Username, keyFile))
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
                throw new SshProviderException("internal_error", $"Agent response on server {endpoint.ServerId} too large.");
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
            throw new SshProviderException(SshProviderException.Unreachable, $"SSH agent on server {endpoint.ServerId} unreachable.");
        }
    }
}

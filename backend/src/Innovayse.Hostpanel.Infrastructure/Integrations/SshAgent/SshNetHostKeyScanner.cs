using System.Net.Sockets;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Ssh;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.SshAgent;

/// <summary>
/// <see cref="ISshHostKeyScanner"/> on SSH.NET. It refuses the host key in the handshake callback,
/// so the connection ends before any authentication: no credential is ever offered to a host
/// nobody has verified yet. The key observed is the one SSH.NET negotiates, i.e. exactly what the
/// agent transport will later compare against the pin.
/// </summary>
public sealed class SshNetHostKeyScanner : ISshHostKeyScanner
{
    /// <summary>Logger.</summary>
    private readonly ILogger<SshNetHostKeyScanner> _logger;

    /// <summary>Initialises the scanner.</summary>
    /// <param name="logger">Logger.</param>
    public SshNetHostKeyScanner(ILogger<SshNetHostKeyScanner> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SshHostKeyScan?> ScanAsync(string host, int port, TimeSpan timeout, CancellationToken ct)
    {
        SshHostKeyScan? seen = null;
        var connection = new ConnectionInfo(host, port, "hostkey-scan", new NoneAuthenticationMethod("hostkey-scan"))
        {
            Timeout = timeout,
        };
        using var client = new SshClient(connection);
        client.HostKeyReceived += (_, e) =>
        {
            seen = new SshHostKeyScan(e.HostKeyName, "SHA256:" + e.FingerPrintSHA256);
            e.CanTrust = false;
        };

        using var limit = CancellationTokenSource.CreateLinkedTokenSource(ct);
        limit.CancelAfter(timeout + TimeSpan.FromSeconds(2));
        try
        {
            await client.ConnectAsync(limit.Token);
        }
        catch (Exception ex) when (ex is SshException or SocketException or IOException or OperationCanceledException or TimeoutException)
        {
            if (seen is null && !ct.IsCancellationRequested)
            {
                _logger.LogInformation(ex, "Host key scan of {Host}:{Port} got no key.", host, port);
            }
        }

        return seen;
    }
}

using System.Net.Sockets;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
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
    public async Task<SshHostKeyProbe> ScanAsync(string host, int port, TimeSpan timeout, CancellationToken ct)
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
                var failure = await ClassifyAsync(ex, host, port, timeout, ct);
                _logger.LogInformation(ex, "Host key scan of {Host}:{Port} got no key: {Failure}.", host, port, failure);
                return new SshHostKeyProbe(null, failure);
            }
        }

        return seen is null ? new SshHostKeyProbe(null, SshEndpointFailure.Other) : new SshHostKeyProbe(seen, null);
    }

    /// <summary>
    /// Tells apart why no key came. A hang-up before the banner is certain from the SSH exception;
    /// anything else is settled by one plain TCP connect, made only on this failure path so a healthy
    /// endpoint is never asked twice (each unauthenticated connection can count against PerSourcePenalties).
    /// </summary>
    /// <param name="ex">What the SSH connect threw.</param>
    /// <param name="host">Host.</param>
    /// <param name="port">Port.</param>
    /// <param name="timeout">Connect timeout.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The failure.</returns>
    private static async Task<SshEndpointFailure> ClassifyAsync(Exception ex, string host, int port, TimeSpan timeout, CancellationToken ct)
    {
        if (ex is SshConnectionException)
        {
            return SshEndpointFailure.ClosedBeforeGreeting;
        }

        if (ex is SocketException { SocketErrorCode: SocketError.ConnectionRefused })
        {
            return SshEndpointFailure.Refused;
        }

        using var tcp = new TcpClient();
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(ct);
        limit.CancelAfter(timeout);
        try
        {
            await tcp.ConnectAsync(host, port, limit.Token);
            return SshEndpointFailure.Other;
        }
        catch (SocketException se) when (se.SocketErrorCode is SocketError.ConnectionRefused)
        {
            return SshEndpointFailure.Refused;
        }
        catch (Exception probe) when (probe is SocketException or OperationCanceledException or IOException)
        {
            return SshEndpointFailure.NoAnswer;
        }
    }
}

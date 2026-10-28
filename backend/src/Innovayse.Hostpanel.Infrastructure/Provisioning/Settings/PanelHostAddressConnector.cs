using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Infrastructure.Provisioning.Settings;

/// <summary>
/// Builds the primary handler for control-panel HTTP clients, dialling hosts listed in
/// <see cref="PanelNetworkOptions"/> at their configured address. Only the TCP connect changes:
/// the request URI, the Host header, SNI and certificate validation all keep the hostname.
/// </summary>
public static class PanelHostAddressConnector
{
    /// <summary>Creates the handler.</summary>
    /// <param name="options">The configured private routes.</param>
    /// <returns>A handler that connects mapped hosts at their mapped address.</returns>
    public static SocketsHttpHandler CreateHandler(IOptions<PanelNetworkOptions> options)
    {
        var map = options.Value.Map;
        var handler = new SocketsHttpHandler();
        if (map.Count == 0)
        {
            return handler;
        }

        handler.ConnectCallback = async (context, ct) =>
        {
            var target = context.DnsEndPoint;
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            try
            {
                if (map.TryGetValue(target.Host, out var address))
                {
                    await socket.ConnectAsync(new IPEndPoint(address, target.Port), ct);
                }
                else
                {
                    await socket.ConnectAsync(target, ct);
                }

                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        };
        return handler;
    }
}

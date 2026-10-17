using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.SDK.Plugins;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Infrastructure.Provisioning;
/// <summary>Tests server connectivity using the appropriate API client based on server module.</summary>
public sealed class ServerConnectionTester : IServerConnectionTester
{
    /// <summary>The cwp client this class uses.</summary>
    private readonly IHostingPanelApiClient _cwpClient;

    /// <summary>The cwp7 client this class uses.</summary>
    private readonly IHostingPanelV7ApiClient _cwp7Client;

    /// <summary>The logger this class uses.</summary>
    private readonly ILogger<ServerConnectionTester> _logger;

    /// <summary>Initializes a new instance of the <see cref="ServerConnectionTester"/> class.</summary>
    /// <param name="cwpClient">The cwp client this class uses.</param>
    /// <param name="cwp7Client">The cwp7 client this class uses.</param>
    /// <param name="logger">The logger this class uses.</param>
    public ServerConnectionTester(IHostingPanelApiClient cwpClient, IHostingPanelV7ApiClient cwp7Client, ILogger<ServerConnectionTester> logger)
    {
        _cwpClient = cwpClient;
        _cwp7Client = cwp7Client;
        _logger = logger;
    }

    /// <summary>
    /// Tests connectivity to the server using its stored credentials.
    /// Supports CWP7 and legacy CentOS modules; other modules return a not-supported error.
    /// </summary>
    /// <param name="server">The server to test.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Connection result tuple.</returns>
    public async Task<(bool Connected, int? AccountsCount, string? Version, string? ErrorMessage)> TestAsync(
        Server server, CancellationToken ct)
    {
        return server.Module switch
        {
            ServerModule.Cwp7 => await TestCwp7Async(server, ct),
            ServerModule.Centos => await TestLegacyCwpAsync(server, ct),
            _ => (false, null, null, $"Connection testing is not yet supported for module '{server.Module}'."),
        };
    }

    /// <summary>
    /// Tests connectivity to a CWP7 server using the IHostingPanelV7ApiClient.
    /// Uses Packages LIST to verify connectivity.
    /// </summary>
    /// <param name="server">The CWP7 server.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Connection result tuple.</returns>
    private async Task<(bool Connected, int? AccountsCount, string? Version, string? ErrorMessage)> TestCwp7Async(
        Server server, CancellationToken ct)
    {
        var apiKey = server.AccessHash;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return (false, null, null, "No access hash configured for this server.");
        }

        const string port = "2304";

        try
        {
            var packagesCount = await _cwp7Client.GetServerInfoAsync(server.Hostname, port, apiKey, ct);
            return (true, packagesCount, null, null);
        }
        catch (Exception ex)
        {
            // The provider's own message never reaches the caller (rules on upstream detail); it
            // is logged here for whoever has to chase the failure up.
            _logger.LogWarning(ex, "CWP7 connection test failed for server {ServerId} ({Hostname})", server.Id, server.Hostname);
            return (false, null, null, "Could not reach the server. See the server log for detail.");
        }
    }

    /// <summary>
    /// Tests connectivity to a legacy CentOS Web Panel server using IHostingPanelApiClient.
    /// </summary>
    /// <param name="server">The legacy CWP server.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Connection result tuple.</returns>
    private async Task<(bool Connected, int? AccountsCount, string? Version, string? ErrorMessage)> TestLegacyCwpAsync(
        Server server, CancellationToken ct)
    {
        var apiKey = server.AccessHash ?? server.ApiToken;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return (false, null, null, "No API key or access hash configured for this server.");
        }

        const string port = "2304";

        try
        {
            var (accountsCount, version) = await _cwpClient.GetServerInfoAsync(server.Hostname, port, apiKey, ct);
            return (true, accountsCount, version, null);
        }
        catch (Exception ex)
        {
            // The provider's own message never reaches the caller (rules on upstream detail); it
            // is logged here for whoever has to chase the failure up.
            _logger.LogWarning(ex, "CWP connection test failed for server {ServerId} ({Hostname})", server.Id, server.Hostname);
            return (false, null, null, "Could not reach the server. See the server log for detail.");
        }
    }
}

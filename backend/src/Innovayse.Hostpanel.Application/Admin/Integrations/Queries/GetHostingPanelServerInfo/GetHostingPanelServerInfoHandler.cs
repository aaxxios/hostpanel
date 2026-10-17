using System.Globalization;
using Innovayse.Hostpanel.Application.Admin.Integrations.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.SDK.Plugins;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Admin.Integrations.Queries.GetHostingPanelServerInfo;

/// <summary>Handles <see cref="GetHostingPanelServerInfoQuery"/> -- fetches live hosting panel server status, cached for 5 minutes.</summary>
public sealed class GetHostingPanelServerInfoHandler
{
    /// <summary>Cache key used to store and retrieve the hosting panel server info result.</summary>
    private const string CacheKey = HostingPanelCacheKeys.Cwp;

    /// <summary>How long a successful server-info result is cached before being refreshed.</summary>
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Hosting panel API client abstraction.</summary>
    private readonly IHostingPanelApiClient _cwpClient;

    /// <summary>In-memory cache for the server info result.</summary>
    private readonly IMemoryCache _cache;

    /// <summary>Logs a provider failure without putting its detail on the wire.</summary>
    private readonly ILogger<GetHostingPanelServerInfoHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="cwpClient">Hosting panel API client abstraction.</param>
    /// <param name="cache">In-memory cache for the server info result.</param>
    /// <param name="logger">Logs a provider failure without putting its detail on the wire.</param>
    public GetHostingPanelServerInfoHandler(
        IHostpanelDbContext context,
        IHostingPanelApiClient cwpClient,
        IMemoryCache cache,
        ILogger<GetHostingPanelServerInfoHandler> logger)
    {
        _context = context;
        _cwpClient = cwpClient;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Fetches hosting panel server status from the in-memory cache if available, or queries the live API.
    /// </summary>
    /// <param name="query">The query (carries no parameters).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The current status, always succeeding. When credentials are missing or the API call fails,
    /// <see cref="HostingPanelServerInfoDto.Connected"/> is <see langword="false"/> rather than the request
    /// being refused -- the diagnostic result is the answer the caller asked for.
    /// </returns>
    public async Task<Result<HostingPanelServerInfoDto>> HandleAsync(GetHostingPanelServerInfoQuery query, CancellationToken ct)
    {
        if (_cache.TryGetValue(CacheKey, out HostingPanelServerInfoDto? cached) && cached is not null)
        {
            return Result<HostingPanelServerInfoDto>.Success(cached);
        }

        var host = await _context.GetIntegrationValueAsync(IntegrationSlugs.InnovayseCwp, IntegrationSettingKeys.HostField, ct);
        var portSetting = await _context.GetIntegrationValueAsync(IntegrationSlugs.InnovayseCwp, IntegrationSettingKeys.PortField, ct);
        var apiKey = await _context.GetIntegrationValueAsync(IntegrationSlugs.InnovayseCwp, IntegrationSettingKeys.ApiKeyField, ct);

        // 2031 used to be the fallback here, and it is the hosting panel's own HTTPS UI -- it answers
        // nothing under /v1/, which is the only path this client requests. An operator who had
        // filled in a host and an API key but left the port blank was therefore told the server
        // was unreachable when it was answering perfectly on 2304, the port every other hosting panel caller
        // in this repository already used.
        var port = portSetting is { Length: > 0 } configured
            ? configured
            : IHostingPanelApiClient.DefaultApiPort.ToString(CultureInfo.InvariantCulture);

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(apiKey))
        {
            return Result<HostingPanelServerInfoDto>.Success(new HostingPanelServerInfoDto(
                Connected: false,
                AccountsCount: null,
                CwpVersion: null,
                LastTestedAt: null,
                ErrorMessage: "CWP is not configured."));
        }

        try
        {
            var (accountsCount, cwpVersion) = await _cwpClient.GetServerInfoAsync(host, port, apiKey, ct);

            var dto = new HostingPanelServerInfoDto(
                Connected: true,
                AccountsCount: accountsCount,
                CwpVersion: cwpVersion,
                LastTestedAt: DateTimeOffset.UtcNow,
                ErrorMessage: null);

            _cache.Set(CacheKey, dto, CacheDuration);
            return Result<HostingPanelServerInfoDto>.Success(dto);
        }
        catch (Exception ex)
        {
            // The provider's own message never reaches the caller (rules on upstream detail); it
            // is logged here for whoever has to chase the failure up.
            _logger.LogWarning(ex, "CWP server-info probe failed.");
            return Result<HostingPanelServerInfoDto>.Success(new HostingPanelServerInfoDto(
                Connected: false,
                AccountsCount: null,
                CwpVersion: null,
                LastTestedAt: null,
                ErrorMessage: "Could not reach the server. See the server log for detail."));
        }
    }
}

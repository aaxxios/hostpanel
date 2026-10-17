using Innovayse.Hostpanel.Application.Admin.Integrations.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.SDK.Plugins;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Admin.Integrations.Queries.GetHostingPanelV7ServerInfo;

/// <summary>Handles <see cref="GetHostingPanelV7ServerInfoQuery"/> -- fetches live hosting panel (v7) server status, cached for 5 minutes.</summary>
public sealed class GetHostingPanelV7ServerInfoHandler
{
    /// <summary>Cache key used to store and retrieve the hosting panel (v7) server info result.</summary>
    private const string CacheKey = HostingPanelCacheKeys.Cwp7;

    /// <summary>How long a successful server-info result is cached before being refreshed.</summary>
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Hosting panel (v7) API client abstraction.</summary>
    private readonly IHostingPanelV7ApiClient _cwp7Client;

    /// <summary>In-memory cache for the server info result.</summary>
    private readonly IMemoryCache _cache;

    /// <summary>Logs a provider failure without putting its detail on the wire.</summary>
    private readonly ILogger<GetHostingPanelV7ServerInfoHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="cwp7Client">Hosting panel (v7) API client abstraction.</param>
    /// <param name="cache">In-memory cache for the server info result.</param>
    /// <param name="logger">Logs a provider failure without putting its detail on the wire.</param>
    public GetHostingPanelV7ServerInfoHandler(
        IHostpanelDbContext context,
        IHostingPanelV7ApiClient cwp7Client,
        IMemoryCache cache,
        ILogger<GetHostingPanelV7ServerInfoHandler> logger)
    {
        _context = context;
        _cwp7Client = cwp7Client;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Fetches hosting panel (v7) server status from the in-memory cache if available, or queries the live API.
    /// Uses Packages LIST to verify connectivity since Account LIST is not enabled.
    /// </summary>
    /// <param name="query">The query (carries no parameters).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The current status, always succeeding. When credentials are missing or the API call fails,
    /// <see cref="HostingPanelV7ServerInfoDto.Connected"/> is <see langword="false"/> rather than the request
    /// being refused -- the diagnostic result is the answer the caller asked for.
    /// </returns>
    public async Task<Result<HostingPanelV7ServerInfoDto>> HandleAsync(GetHostingPanelV7ServerInfoQuery query, CancellationToken ct)
    {
        if (_cache.TryGetValue(CacheKey, out HostingPanelV7ServerInfoDto? cached) && cached is not null)
        {
            return Result<HostingPanelV7ServerInfoDto>.Success(cached);
        }

        var host = await _context.GetIntegrationValueAsync(IntegrationSlugs.Cwp7, IntegrationSettingKeys.HostnameField, ct);
        var portSetting = await _context.GetIntegrationValueAsync(IntegrationSlugs.Cwp7, IntegrationSettingKeys.PortField, ct);
        var apiKey = await _context.GetIntegrationValueAsync(IntegrationSlugs.Cwp7, IntegrationSettingKeys.AccessHashField, ct);

        var port = portSetting ?? "2304";

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(apiKey))
        {
            return Result<HostingPanelV7ServerInfoDto>.Success(new HostingPanelV7ServerInfoDto(
                Connected: false,
                PackagesCount: null,
                LastTestedAt: null,
                ErrorMessage: "CWP7 is not configured."));
        }

        try
        {
            var packagesCount = await _cwp7Client.GetServerInfoAsync(host, port, apiKey, ct);

            var dto = new HostingPanelV7ServerInfoDto(
                Connected: true,
                PackagesCount: packagesCount,
                LastTestedAt: DateTimeOffset.UtcNow,
                ErrorMessage: null);

            _cache.Set(CacheKey, dto, CacheDuration);
            return Result<HostingPanelV7ServerInfoDto>.Success(dto);
        }
        catch (Exception ex)
        {
            // The provider's own message never reaches the caller (rules on upstream detail); it
            // is logged here for whoever has to chase the failure up.
            _logger.LogWarning(ex, "CWP7 server-info probe failed.");
            return Result<HostingPanelV7ServerInfoDto>.Success(new HostingPanelV7ServerInfoDto(
                Connected: false,
                PackagesCount: null,
                LastTestedAt: null,
                ErrorMessage: "Could not reach the server. See the server log for detail."));
        }
    }
}

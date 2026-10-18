using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetSslMonitoring;

/// <summary>Handles <see cref="GetSslMonitoringQuery"/>.</summary>
public sealed class GetSslMonitoringHandler
{
    /// <summary>Certificate check cache over the panel domains.</summary>
    private readonly ISslMonitoringService _sslService;

    /// <summary>Initializes the handler.</summary>
    /// <param name="sslService">Certificate check cache over the panel domains.</param>
    public GetSslMonitoringHandler(ISslMonitoringService sslService)
    {
        _sslService = sslService;
    }

    /// <summary>
    /// Returns the last certificate check for every domain. Reads the cache only, so opening the
    /// report never waits on a live TLS handshake per domain; re-checking is a separate command.
    /// </summary>
    /// <param name="query">Whether inactive domains are included.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Domains grouped by how soon their certificate expires. Never refuses.</returns>
    public async Task<Result<SslMonitoringDto>> HandleAsync(GetSslMonitoringQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var report = await _sslService.GetReportAsync(query.IncludeInactive, ct);
        return Result<SslMonitoringDto>.Success(report);
    }
}

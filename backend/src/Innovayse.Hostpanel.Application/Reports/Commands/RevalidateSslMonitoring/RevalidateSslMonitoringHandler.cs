using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Reports.Commands.RevalidateSslMonitoring;

/// <summary>Handles <see cref="RevalidateSslMonitoringCommand"/>.</summary>
public sealed class RevalidateSslMonitoringHandler
{
    /// <summary>Certificate check cache over the panel domains.</summary>
    private readonly ISslMonitoringService _sslService;

    /// <summary>Initializes the handler.</summary>
    /// <param name="sslService">Certificate check cache over the panel domains.</param>
    public RevalidateSslMonitoringHandler(ISslMonitoringService sslService)
    {
        _sslService = sslService;
    }

    /// <summary>
    /// Re-checks every domain certificate, replaces the cache, and answers with the refreshed
    /// report so the caller does not need a second round trip to read it.
    /// </summary>
    /// <param name="command">Whether inactive domains are checked as well.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The refreshed report, grouped by expiry bucket. Never refuses: a domain that cannot be reached is recorded as having no certificate.</returns>
    public async Task<Result<SslMonitoringDto>> HandleAsync(RevalidateSslMonitoringCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var report = await _sslService.RevalidateAsync(command.IncludeInactive, ct);
        return Result<SslMonitoringDto>.Success(report);
    }
}

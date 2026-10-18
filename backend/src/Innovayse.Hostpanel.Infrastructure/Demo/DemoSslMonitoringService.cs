using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Infrastructure.Monitoring;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Certificate monitoring for the public demo. Reads the stored check results like the real
/// service, but "revalidate" only re-reads them: no connection is opened to any host.
/// </summary>
internal sealed class DemoSslMonitoringService : ISslMonitoringService
{
    /// <summary>The real service, used for its read side only.</summary>
    private readonly SslMonitoringService _inner;

    /// <summary>Creates the service.</summary>
    /// <param name="inner">The real service, used for its read side only.</param>
    public DemoSslMonitoringService(SslMonitoringService inner)
    {
        _inner = inner;
    }

    /// <inheritdoc/>
    public Task<SslMonitoringDto> GetReportAsync(bool includeInactive, CancellationToken ct) =>
        _inner.GetReportAsync(includeInactive, ct);

    /// <inheritdoc/>
    public Task<SslMonitoringDto> RevalidateAsync(bool includeInactive, CancellationToken ct) =>
        _inner.GetReportAsync(includeInactive, ct);
}

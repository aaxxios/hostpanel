using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Infrastructure.Monitoring;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Disk usage reporting for the public demo. Reads the stored figures like the real service, but
/// "update now" only re-reads them: no server is polled.
/// </summary>
internal sealed class DemoDiskUsageService : IDiskUsageService
{
    /// <summary>The real service, used for its read side only.</summary>
    private readonly DiskUsageService _inner;

    /// <summary>Creates the service.</summary>
    /// <param name="inner">The real service, used for its read side only.</param>
    public DemoDiskUsageService(DiskUsageService inner)
    {
        _inner = inner;
    }

    /// <inheritdoc/>
    public Task<DiskUsageDto> GetReportAsync(CancellationToken ct) => _inner.GetReportAsync(ct);

    /// <inheritdoc/>
    public Task<DiskUsageDto> UpdateNowAsync(CancellationToken ct) => _inner.GetReportAsync(ct);
}

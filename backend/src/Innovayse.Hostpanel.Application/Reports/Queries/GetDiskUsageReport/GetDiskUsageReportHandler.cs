using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetDiskUsageReport;

/// <summary>Handles <see cref="GetDiskUsageReportQuery"/>.</summary>
public sealed class GetDiskUsageReportHandler
{
    /// <summary>Disk and bandwidth usage cache over the hosting servers.</summary>
    private readonly IDiskUsageService _diskService;

    /// <summary>Initializes the handler.</summary>
    /// <param name="diskService">Disk and bandwidth usage cache over the hosting servers.</param>
    public GetDiskUsageReportHandler(IDiskUsageService diskService)
    {
        _diskService = diskService;
    }

    /// <summary>
    /// Returns the cached disk and bandwidth figures. Reads the cache only, so an admin opening
    /// the report never waits on every hosting server; refreshing is a separate command.
    /// </summary>
    /// <param name="query">The query. Carries no filters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Accounts grouped by server, with the time the cache was last refreshed. Never refuses.</returns>
    public async Task<Result<DiskUsageDto>> HandleAsync(GetDiskUsageReportQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var report = await _diskService.GetReportAsync(ct);
        return Result<DiskUsageDto>.Success(report);
    }
}

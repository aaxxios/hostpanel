namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>
/// The latest disk and bandwidth figures the hosting server reported for a service's account, in
/// megabytes. A limit of 0 means the server reported no limit.
/// </summary>
/// <param name="DiskUsedMb">Disk space in use.</param>
/// <param name="DiskLimitMb">Disk space allowed; 0 when unlimited.</param>
/// <param name="BandwidthUsedMb">Bandwidth used this period.</param>
/// <param name="BandwidthLimitMb">Bandwidth allowed; 0 when unlimited.</param>
/// <param name="UpdatedAt">When the server last reported these figures.</param>
public sealed record ServiceUsageDto(
    long DiskUsedMb,
    long DiskLimitMb,
    long BandwidthUsedMb,
    long BandwidthLimitMb,
    DateTimeOffset UpdatedAt);

namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>
/// The latest disk and bandwidth figures a hosting server reported for one account, in megabytes.
/// A limit of 0 means the server reported no limit.
/// </summary>
/// <param name="ServerName">The name of the server the account lives on, as the usage sync stores it.</param>
/// <param name="Domain">The account's domain, lower-cased.</param>
/// <param name="DiskUsedMb">Disk space in use.</param>
/// <param name="DiskLimitMb">Disk space allowed; 0 when unlimited.</param>
/// <param name="BandwidthUsedMb">Bandwidth used this period.</param>
/// <param name="BandwidthLimitMb">Bandwidth allowed; 0 when unlimited.</param>
/// <param name="UpdatedAt">When the server last reported these figures.</param>
public sealed record DomainDiskUsageDto(
    string ServerName,
    string Domain,
    long DiskUsedMb,
    long DiskLimitMb,
    long BandwidthUsedMb,
    long BandwidthLimitMb,
    DateTimeOffset UpdatedAt);

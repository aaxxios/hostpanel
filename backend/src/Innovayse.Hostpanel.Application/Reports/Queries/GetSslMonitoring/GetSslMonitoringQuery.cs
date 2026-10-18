namespace Innovayse.Hostpanel.Application.Reports.Queries.GetSslMonitoring;

/// <summary>Query for the cached SSL Monitoring report.</summary>
/// <param name="IncludeInactive">Whether domains that are no longer active are listed too.</param>
public sealed record GetSslMonitoringQuery(bool IncludeInactive = false);

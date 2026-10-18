namespace Innovayse.Hostpanel.Application.Reports.Commands.RevalidateSslMonitoring;

/// <summary>Command to re-check every domain certificate and replace the cached SSL report.</summary>
/// <param name="IncludeInactive">Whether domains that are no longer active are checked too.</param>
public sealed record RevalidateSslMonitoringCommand(bool IncludeInactive = false);

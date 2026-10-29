namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>The automatic suspension and termination settings of a service.</summary>
/// <param name="OverrideAutoSuspend">Whether auto-suspend is overridden.</param>
/// <param name="SuspendUntil">Date until which auto-suspend is blocked.</param>
/// <param name="AutoTerminateEndOfCycle">Whether to auto-terminate at end of billing cycle.</param>
/// <param name="AutoTerminateReason">Reason for auto-termination.</param>
public sealed record ServiceAutomationDto(
    bool OverrideAutoSuspend,
    DateTimeOffset? SuspendUntil,
    bool AutoTerminateEndOfCycle,
    string? AutoTerminateReason);

using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Demo.Commands.ResetDemoData;

/// <summary>
/// Scheduled command that wipes the demo's business data and seeds it again. Dispatched by
/// <c>DemoScheduledJobsStartup</c> and re-scheduled by its own handler every
/// <c>Demo:ResetIntervalHours</c> hours. Carries no payload.
/// </summary>
public sealed record ResetDemoDataCommand : IRecurringJob;

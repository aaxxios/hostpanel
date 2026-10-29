using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// The data manager registered whenever demo mode is off: seeding and resetting do nothing, so
/// a regular deployment never has its data touched. Registered so the reset handler always has
/// a manager to be constructed with.
/// </summary>
public sealed class DisabledDemoDataManager : IDemoDataManager
{
    /// <inheritdoc/>
    public Task SeedAsync(CancellationToken ct) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task ResetAsync(CancellationToken ct) => Task.CompletedTask;
}

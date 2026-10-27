using FluentAssertions;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Shared;

/// <summary>
/// Pins <see cref="RecurringJob.RunAsync"/>: the next run is scheduled after every pass, whatever
/// ended it, and the handler itself never throws.
/// </summary>
public sealed class RecurringJobTests
{
    /// <summary>A pass that throws is swallowed and the next run is still scheduled.</summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task FailingPass_StillSchedulesTheNextRun()
    {
        var scheduled = 0;

        await RecurringJob.RunAsync(
            "Job",
            () => throw new InvalidOperationException("database down"),
            () => { scheduled++; return Task.CompletedTask; },
            NullLogger.Instance,
            CancellationToken.None);

        scheduled.Should().Be(1);
    }

    /// <summary>
    /// A pass cut off by its own cancelled token still schedules the next run. Wolverine cancels a
    /// handler's token when the handler outruns its execution timeout, not only when the host stops,
    /// so rethrowing here would end the chain while the host keeps running.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task CancelledPass_StillSchedulesTheNextRun()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var scheduled = 0;

        await RecurringJob.RunAsync(
            "Job",
            () => Task.FromCanceled(cts.Token),
            () => { scheduled++; return Task.CompletedTask; },
            NullLogger.Instance,
            cts.Token);

        scheduled.Should().Be(1);
    }
}

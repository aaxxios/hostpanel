using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Scheduling;

/// <summary>
/// Pins <see cref="IJobLease"/> against PostgreSQL: of several instances that ask for a job at the
/// same moment exactly one gets it, a lease whose holder never gave it back expires, and the record
/// of the last success survives.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class JobLeaseTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    /// <summary>The job every case asks for.</summary>
    private const string Job = "LeaseTestJob";

    /// <summary>The moment every case starts from.</summary>
    private static readonly DateTimeOffset Start = new(2026, 3, 10, 5, 0, 0, TimeSpan.Zero);

    /// <summary>How long a lease lasts in these cases.</summary>
    private static readonly TimeSpan Duration = TimeSpan.FromMinutes(30);

    /// <summary>Forgets the job's record so each case starts from a job that never ran.</summary>
    /// <returns>A task.</returns>
    public async Task InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.JobRuns.ExecuteDeleteAsync();
    }

    /// <summary>Nothing to clean up after a case.</summary>
    /// <returns>A completed task.</returns>
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>Many instances, each on its own connection, ask at once: exactly one holds the lease.</summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task ConcurrentAcquirers_GetExactlyOneLease()
    {
        const int Instances = 12;

        var decisions = await Task.WhenAll(Enumerable.Range(0, Instances).Select(async i =>
        {
            using var scope = factory.Services.CreateScope();
            var lease = scope.ServiceProvider.GetRequiredService<IJobLease>();
            return await lease.TryAcquireAsync(new JobLeaseClaim(Job, $"instance-{i}", Start, Duration), CancellationToken.None);
        }));

        decisions.Count(d => d.Acquired).Should().Be(1);
        decisions.Where(d => !d.Acquired).Should().OnlyContain(d => d.HeldBy != null && d.HeldBy.StartsWith("instance-", StringComparison.Ordinal));
    }

    /// <summary>A holder that crashed never gave the lease back; once it has expired another instance takes it over.</summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task ExpiredLease_IsTakenOver()
    {
        using var scope = factory.Services.CreateScope();
        var lease = scope.ServiceProvider.GetRequiredService<IJobLease>();
        (await lease.TryAcquireAsync(new JobLeaseClaim(Job, "crashed", Start, Duration), CancellationToken.None))
            .Acquired.Should().BeTrue();

        var stillHeld = await lease.TryAcquireAsync(
            new JobLeaseClaim(Job, "other", Start.AddMinutes(10), Duration), CancellationToken.None);
        var afterExpiry = await lease.TryAcquireAsync(
            new JobLeaseClaim(Job, "other", Start + Duration, Duration), CancellationToken.None);

        stillHeld.Acquired.Should().BeFalse();
        stillHeld.HeldBy.Should().Be("crashed");
        afterExpiry.Acquired.Should().BeTrue();
    }

    /// <summary>The record of the last success is kept, and a failed pass does not overwrite it.</summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task LastSuccess_IsRecordedAndSurvivesAFailedPass()
    {
        using var scope = factory.Services.CreateScope();
        var lease = scope.ServiceProvider.GetRequiredService<IJobLease>();
        (await lease.GetLastSucceededAtAsync(Job, CancellationToken.None)).Should().BeNull();

        await lease.TryAcquireAsync(new JobLeaseClaim(Job, "a", Start, Duration), CancellationToken.None);
        await lease.CompleteAsync(new JobLeaseRelease(Job, "a", Start.AddMinutes(1), true, TimeSpan.Zero), CancellationToken.None);
        await lease.TryAcquireAsync(new JobLeaseClaim(Job, "a", Start.AddDays(1), Duration), CancellationToken.None);
        await lease.CompleteAsync(new JobLeaseRelease(Job, "a", Start.AddDays(1).AddMinutes(1), false, TimeSpan.Zero), CancellationToken.None);

        (await lease.GetLastSucceededAtAsync(Job, CancellationToken.None)).Should().Be(Start.AddMinutes(1));
    }

    /// <summary>A holder whose lease was taken over cannot free the new holder's lease.</summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task Complete_ByAHolderThatLostTheLease_LeavesTheNewHolderInPlace()
    {
        using var scope = factory.Services.CreateScope();
        var lease = scope.ServiceProvider.GetRequiredService<IJobLease>();
        await lease.TryAcquireAsync(new JobLeaseClaim(Job, "slow", Start, Duration), CancellationToken.None);
        await lease.TryAcquireAsync(new JobLeaseClaim(Job, "new", Start + Duration, Duration), CancellationToken.None);

        await lease.CompleteAsync(new JobLeaseRelease(Job, "slow", Start.AddHours(1), true, TimeSpan.Zero), CancellationToken.None);

        (await lease.GetLastSucceededAtAsync(Job, CancellationToken.None)).Should().BeNull();
        var blocked = await lease.TryAcquireAsync(
            new JobLeaseClaim(Job, "third", Start + Duration + TimeSpan.FromMinutes(1), Duration), CancellationToken.None);
        blocked.Acquired.Should().BeFalse();
        blocked.HeldBy.Should().Be("new");
    }
}

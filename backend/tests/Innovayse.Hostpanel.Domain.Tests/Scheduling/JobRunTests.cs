using Innovayse.Hostpanel.Domain.Scheduling;

namespace Innovayse.Hostpanel.Domain.Tests.Scheduling;

/// <summary>Unit tests for the <see cref="JobRun"/> lease.</summary>
public sealed class JobRunTests
{
    /// <summary>The moment every case starts from.</summary>
    private static readonly DateTimeOffset Start = new(2026, 3, 10, 5, 0, 0, TimeSpan.Zero);

    /// <summary>How long a lease lasts in these cases.</summary>
    private static readonly TimeSpan Duration = TimeSpan.FromMinutes(30);

    /// <summary>A job that has never run is held by nobody and has never succeeded.</summary>
    [Fact]
    public void Create_StartsFreeAndNeverRun()
    {
        var run = JobRun.Create("Job");

        run.JobName.Should().Be("Job");
        run.LeaseOwner.Should().BeNull();
        run.LeaseUntil.Should().BeNull();
        run.LastStartedAt.Should().BeNull();
        run.LastSucceededAt.Should().BeNull();
    }

    /// <summary>A free job is taken and the start of the pass is recorded.</summary>
    [Fact]
    public void TryAcquire_WhenFree_TakesTheLease()
    {
        var run = JobRun.Create("Job");

        var acquired = run.TryAcquire("a", Start, Duration);

        acquired.Should().BeTrue();
        run.LeaseOwner.Should().Be("a");
        run.LeaseUntil.Should().Be(Start + Duration);
        run.LastStartedAt.Should().Be(Start);
    }

    /// <summary>A second instance is refused while the first holds an unexpired lease.</summary>
    [Fact]
    public void TryAcquire_WhileHeld_IsRefusedAndChangesNothing()
    {
        var run = JobRun.Create("Job");
        run.TryAcquire("a", Start, Duration);

        var acquired = run.TryAcquire("b", Start.AddMinutes(10), Duration);

        acquired.Should().BeFalse();
        run.LeaseOwner.Should().Be("a");
        run.LeaseUntil.Should().Be(Start + Duration);
    }

    /// <summary>A holder that crashed never gives the lease back; once it has expired another instance takes it over.</summary>
    [Fact]
    public void TryAcquire_AfterTheLeaseExpired_TakesItOver()
    {
        var run = JobRun.Create("Job");
        run.TryAcquire("a", Start, Duration);

        var acquired = run.TryAcquire("b", Start + Duration, Duration);

        acquired.Should().BeTrue();
        run.LeaseOwner.Should().Be("b");
    }

    /// <summary>A successful pass is recorded and keeps the job closed for the rest.</summary>
    [Fact]
    public void Complete_AfterSuccess_RecordsItAndKeepsTheRest()
    {
        var run = JobRun.Create("Job");
        run.TryAcquire("a", Start, Duration);
        var finished = Start.AddMinutes(2);

        var released = run.Complete("a", finished, succeeded: true, rest: TimeSpan.FromMinutes(1));

        released.Should().BeTrue();
        run.LastSucceededAt.Should().Be(finished);
        run.LeaseOwner.Should().BeNull();
        run.LeaseUntil.Should().Be(finished.AddMinutes(1));
        run.TryAcquire("b", finished.AddSeconds(30), Duration).Should().BeFalse();
        run.TryAcquire("b", finished.AddMinutes(1), Duration).Should().BeTrue();
    }

    /// <summary>With no rest a finished job is free at once.</summary>
    [Fact]
    public void Complete_WithNoRest_FreesTheJob()
    {
        var run = JobRun.Create("Job");
        run.TryAcquire("a", Start, Duration);

        run.Complete("a", Start.AddMinutes(2), succeeded: true, rest: TimeSpan.Zero);

        run.LeaseUntil.Should().BeNull();
        run.TryAcquire("b", Start.AddMinutes(2), Duration).Should().BeTrue();
    }

    /// <summary>A failed pass frees the job at once and does not count as a success.</summary>
    [Fact]
    public void Complete_AfterFailure_FreesTheJobWithoutRecordingASuccess()
    {
        var run = JobRun.Create("Job");
        run.TryAcquire("a", Start, Duration);

        run.Complete("a", Start.AddMinutes(2), succeeded: false, rest: TimeSpan.FromMinutes(1));

        run.LastSucceededAt.Should().BeNull();
        run.LeaseUntil.Should().BeNull();
        run.TryAcquire("b", Start.AddMinutes(2), Duration).Should().BeTrue();
    }

    /// <summary>An instance whose lease expired and was taken over cannot overwrite the new holder.</summary>
    [Fact]
    public void Complete_ByAnInstanceThatLostTheLease_ChangesNothing()
    {
        var run = JobRun.Create("Job");
        run.TryAcquire("a", Start, Duration);
        run.TryAcquire("b", Start + Duration, Duration);

        var released = run.Complete("a", Start.AddMinutes(45), succeeded: true, rest: TimeSpan.Zero);

        released.Should().BeFalse();
        run.LeaseOwner.Should().Be("b");
        run.LastSucceededAt.Should().BeNull();
    }
}

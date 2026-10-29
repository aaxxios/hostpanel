using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.SharedKernel.Domain;

namespace Innovayse.Hostpanel.Domain.Tests.Migration;

/// <summary>Unit tests for <see cref="MigrationJob"/>.</summary>
public sealed class MigrationJobTests
{
    /// <summary>A new job is pending, has a key, keeps the selection and trims the trailing slash of the source.</summary>
    [Fact]
    public void Create_StartsPendingWithTheSelection()
    {
        var job = NewJob();

        job.Status.Should().Be(MigrationJobStatus.Pending);
        job.Key.Should().HaveLength(32);
        job.SourceUrl.Should().Be("https://source.example.test");
        job.Label.Should().Be("First run");
        job.ExportClients.Should().BeTrue();
        job.ExportInvoices.Should().BeFalse();
        job.ExportNetworkIssues.Should().BeTrue();
    }

    /// <summary>Two jobs never share a key.</summary>
    [Fact]
    public void Create_GivesEachJobItsOwnKey()
    {
        NewJob().Key.Should().NotBe(NewJob().Key);
    }

    /// <summary>A ping records when the source last answered.</summary>
    [Fact]
    public void RecordPing_StampsTheLastPing()
    {
        var job = NewJob();

        var pingedAt = new DateTimeOffset(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);

        job.RecordPing(pingedAt);

        job.LastPingAt.Should().Be(pingedAt);
    }

    /// <summary>Start moves a queued job in progress and records the totals.</summary>
    [Fact]
    public void Start_RecordsTotalsAndMovesInProgress()
    {
        var job = QueuedJob();

        job.Start(MigrationTotals.None with { Clients = 10, NetworkIssues = 5 });

        job.Status.Should().Be(MigrationJobStatus.InProgress);
        job.ClientsTotal.Should().Be(10);
        job.NetworkIssuesTotal.Should().Be(5);
    }

    /// <summary>Progress lands on the counters of the entity type it names and nowhere else.</summary>
    [Fact]
    public void UpdateProgress_SetsOnlyTheNamedCounters()
    {
        var job = QueuedJob();
        job.Start(MigrationTotals.None with { Clients = 10, Invoices = 10 });

        job.UpdateProgress(MigrationEntityType.Clients, new MigrationEntityCounts(6, 2, 1));

        job.ClientsImported.Should().Be(6);
        job.ClientsSkipped.Should().Be(2);
        job.ClientsFailed.Should().Be(1);
        job.InvoicesImported.Should().Be(0);
        job.InvoicesSkipped.Should().Be(0);
    }

    /// <summary>The overall percentage counts imported, skipped and failed rows against every total.</summary>
    [Fact]
    public void OverallPercent_CountsProcessedRowsAgainstAllTotals()
    {
        var job = QueuedJob();
        job.Start(MigrationTotals.None with { Clients = 10, Invoices = 10 });

        job.UpdateProgress(MigrationEntityType.Clients, new MigrationEntityCounts(7, 2, 1));
        job.UpdateProgress(MigrationEntityType.Invoices, new MigrationEntityCounts(5, 0, 0));

        job.OverallPercent().Should().Be(75);
    }

    /// <summary>A job with nothing to import reports zero rather than dividing by zero.</summary>
    [Fact]
    public void OverallPercent_IsZeroWithNoTotals()
    {
        NewJob().OverallPercent().Should().Be(0);
    }

    /// <summary>A completed job reports 100 percent and a completion time.</summary>
    [Fact]
    public void Complete_FinishesTheJob()
    {
        var job = QueuedJob();
        job.Start(MigrationTotals.None with { Clients = 10 });

        var completedAt = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);

        job.Complete(completedAt);

        job.Status.Should().Be(MigrationJobStatus.Completed);
        job.CompletedAt.Should().Be(completedAt);
        job.OverallPercent().Should().Be(100);
    }

    /// <summary>A failed job keeps the reason and a completion time.</summary>
    [Fact]
    public void Fail_RecordsTheReason()
    {
        var job = QueuedJob();

        var failedAt = new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero);

        job.Fail(MigrationJobFailure.SourceUnreachable, failedAt);

        job.Status.Should().Be(MigrationJobStatus.Failed);
        job.Failure.Should().Be(MigrationJobFailure.SourceUnreachable);
        job.CompletedAt.Should().Be(failedAt);
    }

    /// <summary>A pending job is queued; a failed job is queued again with its failure and counters cleared.</summary>
    [Fact]
    public void Queue_FromPendingOrFailed_MovesToQueuedAndClearsTheLastRun()
    {
        var job = QueuedJob();
        job.Start(MigrationTotals.None with { Clients = 10 });
        job.UpdateProgress(MigrationEntityType.Clients, new MigrationEntityCounts(3, 1, 1));
        job.Fail(MigrationJobFailure.SourceRefused, DateTimeOffset.UnixEpoch);

        job.Queue(DateTimeOffset.UnixEpoch);

        job.Status.Should().Be(MigrationJobStatus.Queued);
        job.Failure.Should().BeNull();
        job.CompletedAt.Should().BeNull();
        job.ClientsImported.Should().Be(0);
        job.ClientsSkipped.Should().Be(0);
        job.ClientsFailed.Should().Be(0);
    }

    /// <summary>A job that is queued, running or completed cannot be queued again.</summary>
    [Fact]
    public void Queue_WhenAlreadyQueuedOrRunningOrCompleted_Throws()
    {
        var job = QueuedJob();
        AssertRefused(() => job.Queue(DateTimeOffset.UnixEpoch));

        job.Start(MigrationTotals.None);
        AssertRefused(() => job.Queue(DateTimeOffset.UnixEpoch));

        job.Complete(DateTimeOffset.UnixEpoch);
        AssertRefused(() => job.Queue(DateTimeOffset.UnixEpoch));
    }

    /// <summary>Queuing starts the heartbeat from the queuing moment, and a retry restarts it.</summary>
    [Fact]
    public void Queue_StartsTheHeartbeatAtTheQueuingMoment()
    {
        var queuedAt = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
        var job = NewJob();

        job.Queue(queuedAt);

        job.LastHeartbeatAt.Should().Be(queuedAt);

        job.Fail(MigrationJobFailure.Interrupted, queuedAt.AddMinutes(30));
        job.Queue(queuedAt.AddHours(1));

        job.LastHeartbeatAt.Should().Be(queuedAt.AddHours(1));
    }

    /// <summary>A heartbeat moves the last-heartbeat moment of a queued or running job.</summary>
    [Fact]
    public void RecordHeartbeat_WhenActive_StoresTheMoment()
    {
        var job = QueuedJob();
        var first = new DateTimeOffset(2026, 10, 1, 9, 1, 0, TimeSpan.Zero);
        var second = first.AddSeconds(30);

        job.RecordHeartbeat(first);
        job.Start(MigrationTotals.None);
        job.RecordHeartbeat(second);

        job.LastHeartbeatAt.Should().Be(second);
    }

    /// <summary>A job that is not running anywhere takes no heartbeat.</summary>
    [Fact]
    public void RecordHeartbeat_WhenNotActive_Throws()
    {
        var job = QueuedJob();
        job.Fail(MigrationJobFailure.Interrupted, DateTimeOffset.UnixEpoch);

        AssertRefused(() => job.RecordHeartbeat(DateTimeOffset.UnixEpoch));
        AssertRefused(() => NewJob().RecordHeartbeat(DateTimeOffset.UnixEpoch));
    }

    /// <summary>Only a queued job can start.</summary>
    [Fact]
    public void Start_WhenNotQueued_Throws()
    {
        AssertRefused(() => NewJob().Start(MigrationTotals.None));
    }

    /// <summary>Progress and completion need a running job.</summary>
    [Fact]
    public void UpdateProgressAndComplete_WhenNotInProgress_Throw()
    {
        var job = QueuedJob();

        AssertRefused(() => job.UpdateProgress(MigrationEntityType.Clients, new MigrationEntityCounts(1, 0, 0)));
        AssertRefused(() => job.Complete(DateTimeOffset.UnixEpoch));
    }

    /// <summary>Only a queued or running job can fail.</summary>
    [Fact]
    public void Fail_WhenNotActive_Throws()
    {
        AssertRefused(() => NewJob().Fail(MigrationJobFailure.ImportFailed, DateTimeOffset.UnixEpoch));
    }

    /// <summary>An entity type or failure outside the enum is refused, never silently ignored.</summary>
    [Fact]
    public void UnknownEnumValues_Throw()
    {
        var job = QueuedJob();
        job.Start(MigrationTotals.None);

        var progress = () => job.UpdateProgress((MigrationEntityType)999, new MigrationEntityCounts(1, 0, 0));
        var fail = () => job.Fail((MigrationJobFailure)999, DateTimeOffset.UnixEpoch);

        progress.Should().Throw<ArgumentOutOfRangeException>();
        fail.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>Asserts the step is refused as an invalid transition.</summary>
    /// <param name="step">The step to run.</param>
    private static void AssertRefused(Action step)
    {
        step.Should().Throw<DomainException>().Which.Code.Should().Be(MigrationJobErrors.InvalidTransition);
    }

    /// <summary>Builds a job and queues it, the state a pull starts from.</summary>
    /// <returns>The queued job.</returns>
    private static MigrationJob QueuedJob()
    {
        var job = NewJob();
        job.Queue(DateTimeOffset.UnixEpoch);
        return job;
    }

    /// <summary>Builds a job that exports everything except invoices.</summary>
    /// <returns>The job.</returns>
    private static MigrationJob NewJob()
    {
        return MigrationJob.Create(new MigrationJobSetup(
            "First run",
            "https://source.example.test/",
            new MigrationExportSelection(
                Clients: true,
                Invoices: false,
                Services: true,
                Domains: true,
                Tickets: true,
                Products: true,
                Orders: true,
                Transactions: true,
                Quotes: true,
                Knowledgebase: true,
                Contacts: true,
                TicketReplies: true,
                Announcements: true,
                Downloads: true,
                NetworkIssues: true)));
    }
}

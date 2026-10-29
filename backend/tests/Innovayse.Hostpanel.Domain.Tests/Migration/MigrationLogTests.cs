using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;

namespace Innovayse.Hostpanel.Domain.Tests.Migration;

/// <summary>Unit tests for <see cref="MigrationLog"/>.</summary>
public sealed class MigrationLogTests
{
    /// <summary>A skipped entry records the job, the entity, the outcome and the skip reason.</summary>
    [Fact]
    public void Create_RecordsTheOutcome()
    {
        var log = MigrationLog.Create(4, MigrationEntityType.Domains, "example.test", MigrationLogAction.Skipped, MigrationSkipReason.AlreadyExists);

        log.JobId.Should().Be(4);
        log.EntityType.Should().Be(MigrationEntityType.Domains);
        log.Identifier.Should().Be("example.test");
        log.Action.Should().Be(MigrationLogAction.Skipped);
        log.SkipReason.Should().Be(MigrationSkipReason.AlreadyExists);
        log.Reason.Should().BeNull();
    }

    /// <summary>A failed entry carries the closed <see cref="MigrationSkipReason.ImportFailed"/> reason and no free text.</summary>
    [Fact]
    public void Create_RecordsAFailureAsImportFailedWithoutDetail()
    {
        var log = MigrationLog.Create(4, MigrationEntityType.Clients, "client-1", MigrationLogAction.Failed, MigrationSkipReason.ImportFailed);

        log.SkipReason.Should().Be(MigrationSkipReason.ImportFailed);
        log.Reason.Should().BeNull();
    }

    /// <summary>Both reasons are optional, so an imported row needs none.</summary>
    [Fact]
    public void Create_LeavesTheReasonsEmptyByDefault()
    {
        var log = MigrationLog.Create(4, MigrationEntityType.Clients, "client-1", MigrationLogAction.Imported);

        log.SkipReason.Should().BeNull();
        log.Reason.Should().BeNull();
    }
}

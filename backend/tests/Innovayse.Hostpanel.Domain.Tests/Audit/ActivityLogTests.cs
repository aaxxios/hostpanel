using Innovayse.Hostpanel.Domain.Audit;

namespace Innovayse.Hostpanel.Domain.Tests.Audit;

/// <summary>Unit tests for the <see cref="ActivityLog"/> entity.</summary>
public sealed class ActivityLogTests
{
    /// <summary>Backdate moves the entry to the day the action was performed and keeps who did it.</summary>
    [Fact]
    public void Backdate_MovesTheTimestampOnly()
    {
        var entry = ActivityLog.Create(clientId: 1, "Invoice created", "staff-1", "Staff", "staff@example.test", "203.0.113.5");
        var performed = new DateTimeOffset(2026, 7, 7, 9, 0, 0, TimeSpan.Zero);

        entry.Backdate(performed);

        entry.CreatedAt.Should().Be(performed);
        entry.AdminName.Should().Be("Staff");
    }

    /// <summary>A system-wide action is recorded without a client.</summary>
    [Fact]
    public void Create_WithoutAClient_RecordsASystemWideAction()
    {
        var entry = ActivityLog.Create(clientId: null, "Installed plugin demo", "staff-1", "Staff", "staff@example.test", "203.0.113.5");

        entry.ClientId.Should().BeNull();
        entry.Description.Should().Be("Installed plugin demo");
    }
}

using FluentAssertions;
using Innovayse.Hostpanel.Domain.Notifications;
using Xunit;
namespace Innovayse.Hostpanel.Domain.Tests.Notifications;

/// <summary>Unit tests for the <see cref="EmailLog"/> entity.</summary>
public sealed class EmailLogTests
{
    /// <summary>Backdate moves the send time to the day the message went out and keeps the outcome.</summary>
    [Fact]
    public void Backdate_MovesTheSendTimeOnly()
    {
        var log = EmailLog.Create("a@example.test", "Hello", "Body", success: false, error: "Mailbox unavailable.");
        var sent = new DateTimeOffset(2026, 8, 12, 9, 0, 0, TimeSpan.Zero);

        log.Backdate(sent);

        log.SentAt.Should().Be(sent);
        log.Success.Should().BeFalse();
        log.Error.Should().Be("Mailbox unavailable.");
    }
}

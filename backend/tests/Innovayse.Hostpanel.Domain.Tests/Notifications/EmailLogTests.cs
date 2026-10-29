using Innovayse.Hostpanel.Domain.Notifications;
using Innovayse.Hostpanel.Domain.Notifications.Enums;

namespace Innovayse.Hostpanel.Domain.Tests.Notifications;

/// <summary>Unit tests for the <see cref="EmailLog"/> entity.</summary>
public sealed class EmailLogTests
{
    /// <summary>Create stamps the send time the caller passes and records the outcome.</summary>
    [Fact]
    public void Create_StampsTheSendTime()
    {
        var sentAt = new DateTimeOffset(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);

        var log = EmailLog.Create("a@example.test", "Hello", "Body", failure: null, sentAt);

        log.SentAt.Should().Be(sentAt);
        log.Success.Should().BeTrue();
        log.Failure.Should().BeNull();
    }

    /// <summary>Backdate moves the send time to the day the message went out and keeps the outcome.</summary>
    [Fact]
    public void Backdate_MovesTheSendTimeOnly()
    {
        var log = EmailLog.Create("a@example.test", "Hello", "Body", EmailSendFailure.RecipientRejected, new DateTimeOffset(2026, 10, 1, 8, 0, 0, TimeSpan.Zero));
        var sent = new DateTimeOffset(2026, 8, 12, 9, 0, 0, TimeSpan.Zero);

        log.Backdate(sent);

        log.SentAt.Should().Be(sent);
        log.Success.Should().BeFalse();
        log.Failure.Should().Be(EmailSendFailure.RecipientRejected);
    }
}

using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Notifications;
using Innovayse.Hostpanel.Domain.Notifications.Enums;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An <see cref="IEmailSender"/> that records what it was asked to send instead of reaching a real
/// SMTP relay. A fake port standing in for the outbound mail relay, which a test must not call.
/// </summary>
public sealed class FakeEmailSender : IEmailSender
{
    /// <summary>Every message this fake was asked to send, in the order it was asked.</summary>
    private readonly System.Collections.Concurrent.ConcurrentQueue<EmailMessage> _sent = new();

    /// <summary>
    /// A snapshot of every message this fake was asked to send, in the order it was asked. A queue
    /// underneath because event handlers send from background threads while a test reads.
    /// </summary>
    public IReadOnlyList<EmailMessage> Sent => _sent.ToArray();

    /// <summary>When set, every call answers this failure instead of recording the message, as a relay that refuses it.</summary>
    public EmailSendFailure? FailWith { get; init; }

    /// <inheritdoc/>
    public Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken ct)
    {
        if (FailWith is { } failure)
        {
            return Task.FromResult(EmailSendResult.Failed(failure));
        }

        _sent.Enqueue(message);
        return Task.FromResult(EmailSendResult.Sent);
    }
}

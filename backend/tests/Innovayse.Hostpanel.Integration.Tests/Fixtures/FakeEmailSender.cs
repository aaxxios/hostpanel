using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Notifications;

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

    /// <inheritdoc/>
    public Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        _sent.Enqueue(message);
        return Task.CompletedTask;
    }
}

using Innovayse.Hostpanel.Domain.Notifications.Enums;
using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Notifications;

/// <summary>
/// Records the outcome of an email send attempt.
/// Used for audit trailing and delivery diagnostics.
/// </summary>
public sealed class EmailLog : BaseEntity
{
    /// <summary>Gets the recipient email address.</summary>
    public string To { get; private set; } = string.Empty;

    /// <summary>Gets the email subject that was sent.</summary>
    public string Subject { get; private set; } = string.Empty;

    /// <summary>Gets the rendered email body that was sent.</summary>
    public string Body { get; private set; } = string.Empty;

    /// <summary>Gets the UTC timestamp at which the send was attempted.</summary>
    public DateTimeOffset SentAt { get; private set; }

    /// <summary>Gets a value indicating whether the email was sent successfully.</summary>
    public bool Success { get; private set; }

    /// <summary>Gets why the send failed; <see langword="null"/> on success.</summary>
    public EmailSendFailure? Failure { get; private set; }

    /// <summary>Initialises a new <see cref="EmailLog"/> for EF Core materialisation.</summary>
    internal EmailLog()
    {
    }

    /// <summary>Initialises an <see cref="EmailLog"/> with the given identity.</summary>
    /// <param name="id">The entity identifier.</param>
    private EmailLog(int id)
    {
        Id = id;
    }

    /// <summary>Moves the send time to the moment the message was really sent, for log lines seeded from history.</summary>
    /// <param name="sentAt">The moment the message was sent (UTC).</param>
    public void Backdate(DateTimeOffset sentAt) => SentAt = sentAt;

    /// <summary>
    /// Creates a new email log entry recording the result of a send attempt.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="body">The rendered email body.</param>
    /// <param name="failure">Why the send failed; <see langword="null"/> when the relay accepted it.</param>
    /// <param name="sentAt">When the send was attempted, UTC.</param>
    /// <returns>A new <see cref="EmailLog"/> stamped with <paramref name="sentAt"/>, successful exactly when <paramref name="failure"/> is null.</returns>
    public static EmailLog Create(string to, string subject, string body, EmailSendFailure? failure, DateTimeOffset sentAt)
    {
        return new EmailLog(0)
        {
            To = to,
            Subject = subject,
            Body = body,
            SentAt = sentAt,
            Success = failure is null,
            Failure = failure,
        };
    }
}

using System.Net;
using System.Text;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Domain.Notifications;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Application.Support.Commands.SendContactMessage;

/// <summary>
/// Relays one public contact-form submission to the operator's enquiry inbox through
/// <see cref="IEmailSender"/>, then announces it on the operator's chat through
/// <see cref="IContactNotifier"/>.
/// </summary>
/// <remarks>
/// <para>
/// The public site posts here, so the one configured relay, <c>SmtpEmailSender</c>, sends
/// everything this platform sends and no other container holds the mail credentials.
/// </para>
/// <para>
/// <b>The mail is not best-effort.</b> Every way it can fail answers a refusal, and the API layer
/// turns each into a status and a code; incomplete mail configuration is a refusal, never a
/// silent success. A caller of this handler that gets success back can rely on
/// the relay having accepted the message.
/// </para>
/// <para>
/// <b>The chat post is</b>, and only it. It happens after the mail and its failure is logged and
/// dropped: the enquiry inbox is the delivery the operator's process is built on, so an enquiry
/// whose mail was accepted has arrived and a chat channel outage must not report it as lost.
/// </para>
/// <para>
/// Field shape (required, length) is now a <see cref="SendContactMessageValidator"/> rule, run by
/// Wolverine's <c>ValidationPolicy</c> before this handler ever runs. What is left here is not a
/// field-shape rule: whether this deployment has a configured enquiry inbox
/// (<c>CONTACT_NOT_CONFIGURED</c>) and whether the relay accepted the message
/// (<c>CONTACT_SEND_FAILED</c>) are both answers only the handler can give.
/// </para>
/// </remarks>
public sealed class SendContactMessageHandler
{
    /// <summary>The one configured SMTP relay.</summary>
    private readonly IEmailSender _emailSender;

    /// <summary>The operator's chat channel, notified after the mail is away.</summary>
    private readonly IContactNotifier _contactNotifier;

    /// <summary>Notification recipients, including the enquiry inbox.</summary>
    private readonly IOptions<NotificationOptions> _options;

    /// <summary>Logger, for the failures an operator has to see.</summary>
    private readonly ILogger<SendContactMessageHandler> _logger;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="emailSender">The one configured SMTP relay.</param>
    /// <param name="contactNotifier">The operator's chat channel, notified after the mail is away.</param>
    /// <param name="options">Notification recipients, including the enquiry inbox.</param>
    /// <param name="logger">Logger, for the failures an operator has to see.</param>
    /// <param name="clock">The source of the current moment.</param>
    public SendContactMessageHandler(
        IEmailSender emailSender,
        IContactNotifier contactNotifier,
        IOptions<NotificationOptions> options,
        ILogger<SendContactMessageHandler> logger,
        IClock clock)
    {
        _emailSender = emailSender;
        _contactNotifier = contactNotifier;
        _options = options;
        _logger = logger;
        _clock = clock;
    }

    /// <summary>Handles <see cref="SendContactMessageCommand"/>.</summary>
    /// <param name="cmd">The submitted form.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success once the relay has accepted the message and the chat post has been attempted
    /// (whether that post succeeded is deliberately not visible to the caller); <c>CONTACT_NOT_CONFIGURED</c>
    /// when this deployment configured no enquiry inbox; <c>CONTACT_SEND_FAILED</c> when the relay
    /// refused the message.
    /// </returns>
    public async Task<Result> HandleAsync(SendContactMessageCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var name = cmd.Name.Trim();
        var email = cmd.Email.Trim();
        var message = cmd.Message.Trim();
        var phone = Trimmed(cmd.Phone);
        var service = Trimmed(cmd.Service);
        var submittedAt = Trimmed(cmd.SubmittedAt) ?? _clock.UtcNow.ToString("u");

        var recipient = _options.Value.ContactEmail;
        if (string.IsNullOrWhiteSpace(recipient))
        {
            // Error, not Warning: the form is visibly broken for every visitor until this is set,
            // and the setting is named here because nothing else in the process will name it.
            _logger.LogError(
                "Contact form submitted but the contact recipient is not configured; the message was not delivered.");

            return Result.FailureCode(HostpanelErrorCodes.CONTACT_NOT_CONFIGURED);
        }

        var subject = $"Website enquiry from {name}";
        var body = BuildBody(name, email, phone, service, message, submittedAt);

        var sent = await _emailSender.SendAsync(new EmailMessage(recipient, subject, body), ct);
        if (!sent.Delivered)
        {
            // The adapter has logged the relay's own wording; the classified reason is logged here
            // beside the recipient, and neither travels to the visitor.
            _logger.LogError(
                "Contact form message to {Recipient} was refused by the SMTP relay: {Failure}.", recipient, sent.Failure);
            return Result.FailureCode(HostpanelErrorCodes.CONTACT_SEND_FAILED);
        }

        // Only after the mail is away, and only ever after: the chat post is the operator's
        // convenience and the mail is the record. Attempting it first would let a chat channel outage
        // hold up a delivery that does not depend on it.
        await NotifyChatAsync(
            new ContactEnquiry(
                name, email, phone, service, message, submittedAt),
            ct);

        return Result.Success();
    }

    /// <summary>
    /// Posts the enquiry to the operator's chat, swallowing any failure of that channel alone.
    /// </summary>
    /// <remarks>
    /// The one <c>catch</c> in this handler that does not propagate a refusal, and the reason is
    /// that the mail above already succeeded. Turning a chat outage into a refusal here would
    /// answer the visitor "your message could not be delivered" about a message sitting in the
    /// operator's inbox, and invite them to send it again. <see cref="OperationCanceledException"/>
    /// is excluded because a cancelled request is not a channel failure and must not be logged as
    /// one.
    /// </remarks>
    /// <param name="enquiry">The validated submission.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the attempt is over, successful or not.</returns>
    private async Task NotifyChatAsync(ContactEnquiry enquiry, CancellationToken ct)
    {
        try
        {
            await _contactNotifier.NotifyAsync(enquiry, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Error, not Warning: a channel an operator believes they are watching has stopped
            // working, and nothing else in the process will say so. The sentence states plainly
            // that the enquiry itself is not lost, so this line cannot be read as a lost message.
            _logger.LogError(
                ex,
                "Contact enquiry could not be posted to the operator's chat. It was still delivered to {Recipient} by email.",
                _options.Value.ContactEmail);
        }
    }

    /// <summary>Trims a value, mapping blank to <see langword="null"/>.</summary>
    /// <param name="value">The submitted value, possibly absent.</param>
    /// <returns>The trimmed value, or <see langword="null"/> when nothing was given.</returns>
    private static string? Trimmed(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    /// <summary>
    /// Renders the enquiry as the HTML body of the notification mail.
    /// </summary>
    /// <remarks>
    /// Every visitor-supplied value goes through <see cref="WebUtility.HtmlEncode(string)"/>. The
    /// Nuxt route this replaces interpolated all five straight into its markup, so a message
    /// containing a tag arrived as markup in the operator's mail client.
    /// </remarks>
    /// <param name="name">Sender's name.</param>
    /// <param name="email">Sender's email address.</param>
    /// <param name="phone">Sender's phone number, or <see langword="null"/>.</param>
    /// <param name="service">Service the enquiry is about, or <see langword="null"/>.</param>
    /// <param name="message">The message body.</param>
    /// <param name="submittedAt">The browser's own formatted send time, or the server's when the browser sent none.</param>
    /// <returns>The HTML body.</returns>
    private static string BuildBody(
        string name, string email, string? phone, string? service, string message, string submittedAt)
    {
        var sb = new StringBuilder();
        sb.Append("<h2>New website enquiry</h2>");
        sb.Append("<table cellpadding=\"6\" cellspacing=\"0\" border=\"1\">");
        AppendRow(sb, "Name", name);
        AppendRow(sb, "Email", email);

        if (phone is not null)
        {
            AppendRow(sb, "Phone", phone);
        }

        if (service is not null)
        {
            AppendRow(sb, "Service", service);
        }

        AppendRow(sb, "Submitted", submittedAt);
        sb.Append("</table>");
        sb.Append("<h3>Message</h3>");

        // Newlines become <br> because the body is sent as HTML; without it a paragraphed message
        // arrives as one run-on line.
        sb.Append("<p>").Append(WebUtility.HtmlEncode(message).ReplaceLineEndings("<br>")).Append("</p>");
        return sb.ToString();
    }

    /// <summary>Appends one label/value row to the details table, HTML-encoding both.</summary>
    /// <param name="sb">The body being built.</param>
    /// <param name="label">The row label.</param>
    /// <param name="value">The value, as the visitor typed it.</param>
    private static void AppendRow(StringBuilder sb, string label, string value) =>
        sb.Append("<tr><td><b>").Append(WebUtility.HtmlEncode(label)).Append("</b></td><td>")
          .Append(WebUtility.HtmlEncode(value)).Append("</td></tr>");
}

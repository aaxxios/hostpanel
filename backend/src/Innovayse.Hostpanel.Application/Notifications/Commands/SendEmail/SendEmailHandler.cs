using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Rendering;
using Innovayse.Hostpanel.Domain.Notifications;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;

/// <summary>
/// Sends an email by rendering the requested template and delegating to <see cref="IEmailSender"/>.
/// Always writes an <see cref="EmailLog"/> entry regardless of send outcome.
/// </summary>
public sealed class SendEmailHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The mail relay this platform does not own; failures are recorded, never thrown.</summary>
    private readonly IEmailSender _emailSender;

    /// <summary>Renders a stored template's Liquid syntax against the caller's data model.</summary>
    private readonly TemplateRenderer _renderer;

    /// <summary>Records a missing template or an undelivered message, which every fire-and-forget caller would otherwise lose.</summary>
    private readonly ILogger<SendEmailHandler> _logger;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>The Wolverine bus, for the addresses of the contacts a message is copied to.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="emailSender">The mail relay this platform does not own.</param>
    /// <param name="renderer">Renders a stored template against the caller's data model.</param>
    /// <param name="logger">Records a refusal no caller reads.</param>
    /// <param name="clock">The source of the current moment.</param>
    /// <param name="bus">The Wolverine bus, for the addresses of the contacts a message is copied to.</param>
    public SendEmailHandler(
        IHostpanelDbContext context,
        IEmailSender emailSender,
        TemplateRenderer renderer,
        ILogger<SendEmailHandler> logger,
        IClock clock,
        IMessageBus bus)
    {
        _context = context;
        _emailSender = emailSender;
        _renderer = renderer;
        _logger = logger;
        _clock = clock;
        _bus = bus;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The recipient, template slug and data model to render.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success once the attempt has been logged, whether or not the relay accepted the message:
    /// a delivery failure is recorded on the log entry, not answered as a refusal.
    /// <c>EMAIL_TEMPLATE_NOT_FOUND</c> when the slug has no version in the
    /// recipient's language nor in the default language.
    /// </returns>
    public async Task<Result> HandleAsync(SendEmailCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        // The recipient's version when one is written, the default language's otherwise: a template
        // nobody has translated yet still reaches the recipient rather than being lost.
        var language = SupportedLanguages.IsSupported(cmd.Language?.ToLowerInvariant())
            ? cmd.Language!.ToLowerInvariant()
            : SupportedLanguages.Default;
        var versions = await _context.EmailTemplates
            .Where(t => t.Slug == cmd.TemplateSlug
                && (t.Language == language || t.Language == SupportedLanguages.Default))
            .ToListAsync(ct);
        var template = versions.Find(t => t.Language == language)
            ?? versions.Find(t => t.Language == SupportedLanguages.Default);
        if (template is null)
        {
            // Before the Result conversion this threw and surfaced in the log; every event handler
            // dispatches this and ignores the answer, so without this line the mail is lost silently.
            _logger.LogWarning("Email template {TemplateSlug} was not found; nothing was sent", cmd.TemplateSlug);
            return Result.FailureCode(HostpanelErrorCodes.EMAIL_TEMPLATE_NOT_FOUND);
        }

        if (!template.IsActive)
        {
            return Result.Success();
        }

        var subject = await _renderer.RenderTextAsync(template.Subject, cmd.TemplateData, ct);
        var body = await _renderer.RenderHtmlAsync(template.Body, cmd.TemplateData, ct);

        await DeliverAsync(cmd.To, cmd.TemplateSlug, subject, body, ct);

        // The contacts who switched this kind of mail on get the same message. Each copy is its own
        // delivery and its own log line, and an address already written to is not mailed twice.
        if (cmd.Copy is { } copy)
        {
            var contacts = await _bus.ListContactNotificationEmailsAsync(copy.ClientId, copy.Topic, ct);
            foreach (var address in contacts.Where(a => !string.Equals(a, cmd.To, StringComparison.OrdinalIgnoreCase)))
            {
                await DeliverAsync(address, cmd.TemplateSlug, subject, body, ct);
            }
        }

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }

    /// <summary>Hands one rendered message to the relay and stages its log line.</summary>
    /// <param name="to">The recipient address.</param>
    /// <param name="templateSlug">The template the message was rendered from, for the log.</param>
    /// <param name="subject">The rendered subject.</param>
    /// <param name="body">The rendered HTML body.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the relay has answered; the log line is saved by the caller.</returns>
    private async Task DeliverAsync(string to, string templateSlug, string subject, string body, CancellationToken ct)
    {
        // The relay is a port this platform does not own; a failed send is recorded on the log
        // entry as its classified reason rather than turned into a refusal, so a caller that fires
        // this command and forgets it (every event handler in Notifications/Events does) never
        // sees a mail server being unreachable. The relay's own wording is logged by the adapter.
        var sent = await _emailSender.SendAsync(new EmailMessage(to, subject, body), ct);
        if (!sent.Delivered)
        {
            _logger.LogWarning("Email from template {TemplateSlug} was not delivered: {Failure}", templateSlug, sent.Failure);
        }

        _context.EmailLogs.Add(EmailLog.Create(to, subject, body, sent.Failure, _clock.UtcNow));
    }
}

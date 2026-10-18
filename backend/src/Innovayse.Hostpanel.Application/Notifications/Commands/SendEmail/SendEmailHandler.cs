using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Rendering;
using Innovayse.Hostpanel.Domain.Notifications;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

    /// <summary>Records a missing template, which every fire-and-forget caller would otherwise lose.</summary>
    private readonly ILogger<SendEmailHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="emailSender">The mail relay this platform does not own.</param>
    /// <param name="renderer">Renders a stored template against the caller's data model.</param>
    /// <param name="logger">Records a refusal no caller reads.</param>
    public SendEmailHandler(
        IHostpanelDbContext context,
        IEmailSender emailSender,
        TemplateRenderer renderer,
        ILogger<SendEmailHandler> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _renderer = renderer;
        _logger = logger;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The recipient, template slug and data model to render.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success once the attempt has been logged, whether or not the relay accepted the message --
    /// a delivery failure is recorded on the log entry, not answered as a refusal, exactly as
    /// before this conversion. <c>EMAIL_TEMPLATE_NOT_FOUND</c> when the slug names no template.
    /// </returns>
    public async Task<Result> HandleAsync(SendEmailCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var template = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Slug == cmd.TemplateSlug, ct);
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

        var subject = await _renderer.RenderAsync(template.Subject, cmd.TemplateData);
        var body = await _renderer.RenderAsync(template.Body, cmd.TemplateData);

        string? sendError = null;
        var success = false;

        try
        {
            await _emailSender.SendAsync(new EmailMessage(cmd.To, subject, body), ct);
            success = true;
        }
        catch (Exception ex)
        {
            // The relay is a port this platform does not own; a failed send is recorded on the
            // log entry rather than turned into a refusal, so a caller that fires this command
            // and forgets it (every event handler in Notifications/Events does) never sees an
            // exception for a mail server being unreachable.
            sendError = ex.Message;
        }

        var log = EmailLog.Create(cmd.To, subject, body, success, sendError);
        _context.EmailLogs.Add(log);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}

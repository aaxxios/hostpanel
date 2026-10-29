using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Notifications;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Notifications.Commands.CreateEmailTemplate;

/// <summary>Creates a new email template, or a new language version of one, and persists it.</summary>
public sealed class CreateEmailTemplateHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CreateEmailTemplateHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The create email template command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the new template's id; <c>EMAIL_TEMPLATE_ALREADY_EXISTS</c> when the slug already has
    /// a version in that language.
    /// </returns>
    public async Task<Result<int>> HandleAsync(CreateEmailTemplateCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var template = EmailTemplate.Create(cmd.Slug, cmd.Language, cmd.Subject, cmd.Body, cmd.Description);
        if (await _context.EmailTemplates.AnyAsync(t => t.Slug == template.Slug && t.Language == template.Language, ct))
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.EMAIL_TEMPLATE_ALREADY_EXISTS);
        }

        _context.EmailTemplates.Add(template);
        await _context.SaveChangesAsync(ct);

        return Result.Success(template.Id);
    }
}

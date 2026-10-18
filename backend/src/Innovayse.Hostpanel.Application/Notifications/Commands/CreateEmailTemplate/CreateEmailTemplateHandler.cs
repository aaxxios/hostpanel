using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Notifications;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Notifications.Commands.CreateEmailTemplate;

/// <summary>Creates a new email template and persists it.</summary>
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
    /// <returns>Success with the new template's id. This use case answers no refusal code.</returns>
    public async Task<Result<int>> HandleAsync(CreateEmailTemplateCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var template = EmailTemplate.Create(cmd.Slug, cmd.Subject, cmd.Body, cmd.Description);
        _context.EmailTemplates.Add(template);
        await _context.SaveChangesAsync(ct);

        return Result.Success(template.Id);
    }
}

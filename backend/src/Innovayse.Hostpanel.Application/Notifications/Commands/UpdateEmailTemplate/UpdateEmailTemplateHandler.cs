using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Notifications.Commands.UpdateEmailTemplate;

/// <summary>Updates the content of an existing email template.</summary>
public sealed class UpdateEmailTemplateHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateEmailTemplateHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The template to update and its new content.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>EMAIL_TEMPLATE_NOT_FOUND</c> when the id names no template.</returns>
    public async Task<Result> HandleAsync(UpdateEmailTemplateCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var template = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Id == cmd.Id, ct);
        if (template is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.EMAIL_TEMPLATE_NOT_FOUND);
        }

        template.Update(cmd.Subject, cmd.Body, cmd.Description);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}

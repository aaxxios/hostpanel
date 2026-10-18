using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Notifications;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Notifications.Commands.ToggleEmailTemplate;

/// <summary>Activates or deactivates an email template.</summary>
public sealed class ToggleEmailTemplateHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ToggleEmailTemplateHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The toggle email template command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>EMAIL_TEMPLATE_NOT_FOUND</c> when the id names no template.</returns>
    public async Task<Result> HandleAsync(ToggleEmailTemplateCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var template = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Id == cmd.Id, ct);
        if (template is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.EMAIL_TEMPLATE_NOT_FOUND);
        }

        ApplySwitch(template, cmd.Active);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }

    /// <summary>Activates or deactivates the template, whichever the command asked for.</summary>
    /// <param name="template">The template written by this command.</param>
    /// <param name="active">Whether it should accept sends.</param>
    private static void ApplySwitch(EmailTemplate template, bool active)
    {
        if (active)
        {
            template.Activate();
        }
        else
        {
            template.Deactivate();
        }
    }
}

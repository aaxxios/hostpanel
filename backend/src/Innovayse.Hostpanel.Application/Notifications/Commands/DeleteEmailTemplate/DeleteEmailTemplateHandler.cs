using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Notifications.Commands.DeleteEmailTemplate;

/// <summary>Permanently removes an email template from the store.</summary>
public sealed class DeleteEmailTemplateHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public DeleteEmailTemplateHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The delete email template command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>EMAIL_TEMPLATE_NOT_FOUND</c> when the id names no template.</returns>
    public async Task<Result> HandleAsync(DeleteEmailTemplateCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var template = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Id == cmd.Id, ct);
        if (template is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.EMAIL_TEMPLATE_NOT_FOUND);
        }

        _context.EmailTemplates.Remove(template);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}

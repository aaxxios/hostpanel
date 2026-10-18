using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Notifications.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Notifications.Queries.GetEmailTemplate;

/// <summary>Returns a single email template by its identifier.</summary>
public sealed class GetEmailTemplateHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetEmailTemplateHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The template's primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The template, or <c>EMAIL_TEMPLATE_NOT_FOUND</c> when the id names no template.</returns>
    public async Task<Result<EmailTemplateDto>> HandleAsync(GetEmailTemplateQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var template = await _context.EmailTemplates
            .AsNoTracking()
            .Where(t => t.Id == query.Id)
            .SelectDto()
            .FirstOrDefaultAsync(ct);

        if (template is null)
        {
            return Result<EmailTemplateDto>.FailureCode(HostpanelErrorCodes.EMAIL_TEMPLATE_NOT_FOUND);
        }

        return Result<EmailTemplateDto>.Success(template);
    }
}

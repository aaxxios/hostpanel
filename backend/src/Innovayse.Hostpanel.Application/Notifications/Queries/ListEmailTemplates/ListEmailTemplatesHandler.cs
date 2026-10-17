using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Notifications.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Notifications.Queries.ListEmailTemplates;

/// <summary>Returns all email templates.</summary>
public sealed class ListEmailTemplatesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListEmailTemplatesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The list email templates query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every template, ordered by slug. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<EmailTemplateDto>>> HandleAsync(
        ListEmailTemplatesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var templates = await _context.EmailTemplates
            .AsNoTracking()
            .OrderBy(t => t.Slug)
            .SelectDto()
            .ToListAsync(ct);

        return Result<IReadOnlyList<EmailTemplateDto>>.Success(templates);
    }
}

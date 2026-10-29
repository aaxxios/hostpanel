using System.Globalization;
using Innovayse.Hostpanel.Application.Common.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Application.Projects.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Application.Projects.Queries.ListActiveProjects;

/// <summary>
/// Loads the active projects in display order, resolves the best-match translation for the requested
/// locale, and maps them to <see cref="ProjectPublicDto"/>.
/// </summary>
public sealed class ListActiveProjectsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The panel fallback locale, for content with no translation in the requested one.</summary>
    private readonly IOptions<LocaleOptions> _localeOptions;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="localeOptions">The panel fallback locale, for content with no translation in the requested one.</param>
    public ListActiveProjectsHandler(IHostpanelDbContext context, IOptions<LocaleOptions> localeOptions)
    {
        _context = context;
        _localeOptions = localeOptions;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The query; it carries no input: the locale is the request culture set from <c>X-Language</c>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The active projects ordered by sort order, each with its requested-locale text (falling back to the
    /// default locale, then to whatever is stored). A project with no translation at all is left out. Never refuses.
    /// </returns>
    public async Task<Result<IReadOnlyList<ProjectPublicDto>>> HandleAsync(ListActiveProjectsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await _context.Projects
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        var requested = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var fallback = _localeOptions.Value.DefaultLocale;

        var dtos = new List<ProjectPublicDto>(rows.Count);
        foreach (var e in rows)
        {
            var t = e.Translations.ForLocale(translation => translation.Locale, requested, fallback);
            if (t is null)
            {
                continue;
            }

            dtos.Add(new ProjectPublicDto(
                e.Id,
                e.Slug,
                t.Title,
                t.Summary,
                t.Description,
                e.ImageUrls,
                e.Url,
                e.SortOrder));
        }

        return Result<IReadOnlyList<ProjectPublicDto>>.Success(dtos);
    }
}

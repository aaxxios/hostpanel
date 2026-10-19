using System.Globalization;
using Innovayse.Hostpanel.Application.Common.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Application.Faqs.Queries.ListActiveFaqs;

/// <summary>
/// Loads the active questions in display order, resolves the best-match translation for the requested
/// locale, and maps them to <see cref="FaqPublicDto"/>.
/// </summary>
public sealed class ListActiveFaqsHandler
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
    public ListActiveFaqsHandler(IHostpanelDbContext context, IOptions<LocaleOptions> localeOptions)
    {
        _context = context;
        _localeOptions = localeOptions;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The query; it carries no input — the locale is the request culture set from <c>X-Language</c>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The active questions ordered by sort order, each with its requested-locale text (falling back to the
    /// default locale, then to whatever is stored). A question with no translation at all is left out. Never refuses.
    /// </returns>
    public async Task<Result<IReadOnlyList<FaqPublicDto>>> HandleAsync(ListActiveFaqsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await _context.Faqs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        var requested = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var fallback = _localeOptions.Value.DefaultLocale;

        var dtos = new List<FaqPublicDto>(rows.Count);
        foreach (var e in rows)
        {
            var t = e.Translations.ForLocale(translation => translation.Locale, requested, fallback);
            if (t is null)
            {
                continue;
            }

            dtos.Add(new FaqPublicDto(
                e.Id,
                e.Category,
                t.Question,
                t.Answer,
                e.SortOrder));
        }

        return Result<IReadOnlyList<FaqPublicDto>>.Success(dtos);
    }
}

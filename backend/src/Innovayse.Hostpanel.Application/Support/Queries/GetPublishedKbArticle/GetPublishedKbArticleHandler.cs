using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetPublishedKbArticle;

/// <summary>Returns a knowledge base article only while it is published.</summary>
/// <remarks>
/// The check lives here rather than at the endpoint so it travels with the message: nothing can
/// read an article through <see cref="GetPublishedKbArticleQuery"/> without it having run. The
/// row is projected through the shared <see cref="KbArticleMappingExtensions.SelectPublishedDto"/>
/// mapping, so it carries no editorial field.
/// </remarks>
public sealed class GetPublishedKbArticleHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetPublishedKbArticleHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query naming the article to read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The matching article, or <c>KBARTICLE_NOT_FOUND</c>, answered identically
    /// whether the id names no row at all or names an unpublished draft. Ids here are sequential
    /// and this route is anonymous, so telling those two cases apart would let a visitor map the
    /// unpublished backlog one guessed id at a time; the refusal is deliberately indistinguishable.
    /// </returns>
    public async Task<Result<PublishedKbArticleDto>> HandleAsync(GetPublishedKbArticleQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var article = await _context.KbArticles
            .AsNoTracking()
            .Where(a => a.Id == query.Id && a.IsPublished)
            .SelectPublishedDto()
            .FirstOrDefaultAsync(ct);

        if (article is null)
        {
            return Result<PublishedKbArticleDto>.FailureCode(HostpanelErrorCodes.KBARTICLE_NOT_FOUND);
        }

        return Result<PublishedKbArticleDto>.Success(article);
    }
}

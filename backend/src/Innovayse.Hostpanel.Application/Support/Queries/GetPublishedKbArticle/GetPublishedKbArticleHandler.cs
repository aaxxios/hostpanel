using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Queries.GetKbArticle;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetPublishedKbArticle;

/// <summary>Returns a knowledge base article only while it is published.</summary>
/// <remarks>
/// The check lives here rather than at the endpoint so it travels with the message: nothing can
/// read an article through <see cref="GetPublishedKbArticleQuery"/> without it having run. Once
/// visibility is settled the projection is the read the admin route already performs, so this
/// dispatches <see cref="GetKbArticleQuery"/> rather than growing a second copy of the mapping
/// that could drift from it.
/// </remarks>
public sealed class GetPublishedKbArticleHandler
{
    /// <summary>The module's data context, used for the visibility check only.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Wolverine bus, used to reach the shared read once visibility is settled.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context, used for the visibility check only.</param>
    /// <param name="bus">Wolverine bus, used to reach the shared read once visibility is settled.</param>
    public GetPublishedKbArticleHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query naming the article to read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The matching article, or <c>KB_PUBLISHED_ARTICLE_UNAVAILABLE</c> -- answered identically
    /// whether the id names no row at all or names an unpublished draft. Ids here are sequential
    /// and this route is anonymous, so telling those two cases apart would let a visitor map the
    /// unpublished backlog one guessed id at a time; the refusal is deliberately indistinguishable.
    /// </returns>
    public async Task<Result<KbArticleDto>> HandleAsync(GetPublishedKbArticleQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var isPublished = await _context.KbArticles
            .AsNoTracking()
            .Where(a => a.Id == query.Id)
            .Select(a => a.IsPublished)
            .Cast<bool?>()
            .FirstOrDefaultAsync(ct);

        if (isPublished is not true)
        {
            return Result<KbArticleDto>.FailureCode(HostpanelErrorCodes.KB_PUBLISHED_ARTICLE_UNAVAILABLE);
        }

        return await _bus.InvokeAsync<Result<KbArticleDto>>(new GetKbArticleQuery(query.Id), ct);
    }
}

using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateKbArticle;

/// <summary>Updates an existing knowledge base article's title, content, and category.</summary>
public sealed class UpdateKbArticleHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateKbArticleHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The fields to update.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>KBARTICLE_NOT_FOUND</c> when the id names no article.</returns>
    public async Task<Result> HandleAsync(UpdateKbArticleCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var article = await _context.KbArticles.FirstOrDefaultAsync(a => a.Id == cmd.Id, ct);
        if (article is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.KBARTICLE_NOT_FOUND);
        }

        article.Update(cmd.Title, cmd.Content, cmd.Category);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}

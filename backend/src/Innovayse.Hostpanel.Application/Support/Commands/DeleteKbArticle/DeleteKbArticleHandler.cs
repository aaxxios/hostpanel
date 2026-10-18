using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.DeleteKbArticle;

/// <summary>Permanently deletes a knowledge base article.</summary>
public sealed class DeleteKbArticleHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public DeleteKbArticleHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The article to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>KBARTICLE_NOT_FOUND</c> when the id names no article.</returns>
    public async Task<Result> HandleAsync(DeleteKbArticleCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var article = await _context.KbArticles.FirstOrDefaultAsync(a => a.Id == cmd.Id, ct);
        if (article is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.KBARTICLE_NOT_FOUND);
        }

        _context.KbArticles.Remove(article);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}

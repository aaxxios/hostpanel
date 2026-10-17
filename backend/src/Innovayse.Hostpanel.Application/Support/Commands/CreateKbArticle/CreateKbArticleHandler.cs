using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateKbArticle;

/// <summary>Creates a new knowledge base article.</summary>
public sealed class CreateKbArticleHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CreateKbArticleHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The article to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the new article's id. This use case answers no refusal code itself.</returns>
    public async Task<Result<int>> HandleAsync(CreateKbArticleCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var article = KbArticle.Create(cmd.Title, cmd.Content, cmd.Category);
        if (cmd.IsPublished)
        {
            article.Publish();
        }

        _context.KbArticles.Add(article);
        await _context.SaveChangesAsync(ct);

        return Result.Success(article.Id);
    }
}

using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Faqs.Extensions;
using Innovayse.Hostpanel.Domain.Faqs;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Faqs.Commands.CreateFaq;

/// <summary>
/// Creates a new <see cref="Faq"/> aggregate, applies every translation the command carries, and
/// persists the result.
/// </summary>
public sealed class CreateFaqHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public CreateFaqHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The question and its translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created question's id. This use case refuses nothing.</returns>
    public async Task<Result<int>> HandleAsync(CreateFaqCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var entity = Faq.Create(cmd.Category, cmd.SortOrder, cmd.IsActive);
        entity.ReplaceTranslations(cmd.Translations.ToContent());

        _context.Faqs.Add(entity);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(entity.Id);
    }
}

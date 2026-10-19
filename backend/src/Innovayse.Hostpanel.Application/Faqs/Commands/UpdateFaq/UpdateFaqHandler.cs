using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Faqs.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Faqs.Commands.UpdateFaq;

/// <summary>
/// Loads an existing question, applies its new field values, replaces its translations, and persists
/// the result.
/// </summary>
public sealed class UpdateFaqHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public UpdateFaqHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The question's new fields and translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>FAQ_NOT_FOUND</c> when the id names no question.</returns>
    public async Task<Result> HandleAsync(UpdateFaqCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var entity = await _context.Faqs.FirstOrDefaultAsync(x => x.Id == cmd.Id, ct);
        if (entity is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.FAQ_NOT_FOUND);
        }

        entity.Update(cmd.Category, cmd.SortOrder, cmd.IsActive);
        entity.ApplyTranslations(cmd.Translations);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}

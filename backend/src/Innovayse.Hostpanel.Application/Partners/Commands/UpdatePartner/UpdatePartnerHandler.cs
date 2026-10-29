using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Partners.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Partners.Commands.UpdatePartner;

/// <summary>
/// Loads an existing partner, applies its new field values, replaces its translations, and persists
/// the result.
/// </summary>
public sealed class UpdatePartnerHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public UpdatePartnerHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The partner's new fields and translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>PARTNER_NOT_FOUND</c> when the id names no partner.</returns>
    public async Task<Result> HandleAsync(UpdatePartnerCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var entity = await _context.Partners.FirstOrDefaultAsync(x => x.Id == cmd.Id, ct);
        if (entity is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.PARTNER_NOT_FOUND);
        }

        entity.Update(cmd.LogoUrl, cmd.Url, cmd.SortOrder, cmd.IsActive);
        entity.ReplaceTranslations(cmd.Translations.ToContent());

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}

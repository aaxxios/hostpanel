using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Partners.Extensions;
using Innovayse.Hostpanel.Domain.Partners;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Partners.Commands.CreatePartner;

/// <summary>
/// Creates a new <see cref="Partner"/> aggregate, applies every translation the command carries, and
/// persists the result.
/// </summary>
public sealed class CreatePartnerHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public CreatePartnerHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The partner and its translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created partner's id. This use case refuses nothing.</returns>
    public async Task<Result<int>> HandleAsync(CreatePartnerCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var entity = Partner.Create(cmd.LogoUrl, cmd.Url, cmd.SortOrder, cmd.IsActive);
        entity.ApplyTranslations(cmd.Translations);

        _context.Partners.Add(entity);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(entity.Id);
    }
}

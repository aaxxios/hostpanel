using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Application.Admin.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Queries.GetSettings;

/// <summary>
/// Handles <see cref="GetSettingsQuery"/> by returning all configuration settings.
/// </summary>
public sealed class GetSettingsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetSettingsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns all settings ordered by key.
    /// </summary>
    /// <param name="query">The get settings query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with every configured setting. This use case refuses nothing.</returns>
    public async Task<Result<IReadOnlyList<SettingDto>>> HandleAsync(GetSettingsQuery query, CancellationToken ct)
    {
        // Was ISettingRepository.ListAsync -- read-only, so no tracking is needed.
        var settings = await _context.Settings.AsNoTracking().OrderBy(s => s.Key).ToListAsync(ct);
        IReadOnlyList<SettingDto> dtos = settings.Select(s => s.ToDto()).ToList();

        return Result<IReadOnlyList<SettingDto>>.Success(dtos);
    }
}

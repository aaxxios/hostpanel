using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Application.Admin.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Queries.GetSetting;

/// <summary>
/// Handles <see cref="GetSettingQuery"/> by loading a single setting by ID.
/// </summary>
public sealed class GetSettingHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Loaded plugins, whose manifests declare which of their fields are secret.</summary>
    private readonly IPluginRegistry _plugins;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="plugins">Loaded plugins, whose manifests declare which of their fields are secret.</param>
    public GetSettingHandler(IHostpanelDbContext context, IPluginRegistry plugins)
    {
        _context = context;
        _plugins = plugins;
    }

    /// <summary>
    /// Returns a single setting DTO.
    /// </summary>
    /// <param name="query">The get setting query with the ID to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The setting, or <c>SETTING_NOT_FOUND</c> when the id names no setting.</returns>
    public async Task<Result<SettingDto>> HandleAsync(GetSettingQuery query, CancellationToken ct)
    {
        // Was ISettingRepository.FindByIdAsync -- read-only, so no tracking is needed.
        var setting = await _context.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Id == query.Id, ct);
        if (setting is null)
        {
            return Result<SettingDto>.FailureCode(HostpanelErrorCodes.SETTING_NOT_FOUND);
        }

        return Result<SettingDto>.Success(setting.ToDto(SettingSecrecy.From(_plugins)));
    }
}

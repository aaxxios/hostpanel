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

    /// <summary>Loaded plugins, whose manifests declare which of their fields are secret.</summary>
    private readonly IPluginRegistry _plugins;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="plugins">Loaded plugins, whose manifests declare which of their fields are secret.</param>
    public GetSettingsHandler(IHostpanelDbContext context, IPluginRegistry plugins)
    {
        _context = context;
        _plugins = plugins;
    }

    /// <summary>
    /// Returns all settings ordered by key.
    /// </summary>
    /// <param name="query">The get settings query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with every configured setting, secret values withheld. This use case refuses nothing.</returns>
    public async Task<Result<IReadOnlyList<SettingDto>>> HandleAsync(GetSettingsQuery query, CancellationToken ct)
    {
        var settings = await _context.Settings.AsNoTracking().OrderBy(s => s.Key).ToListAsync(ct);
        // A secret row is listed with isSecret and hasValue only; its value never leaves the server.
        var secrecy = SettingSecrecy.From(_plugins);
        IReadOnlyList<SettingDto> dtos = settings.Select(s => s.ToDto(secrecy)).ToList();

        return Result<IReadOnlyList<SettingDto>>.Success(dtos);
    }
}

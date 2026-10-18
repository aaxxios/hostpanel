using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Domain.Settings;

namespace Innovayse.Hostpanel.Application.Admin.Extensions;

/// <summary>
/// Maps a <see cref="Setting"/> to <see cref="SettingDto"/>. The single-setting and the list reads
/// both go through this one mapping, so a field added to the DTO is added once.
/// </summary>
public static class SettingMappingExtensions
{
    /// <summary>Maps a setting to its DTO.</summary>
    /// <param name="setting">The setting.</param>
    /// <returns>The mapped DTO.</returns>
    public static SettingDto ToDto(this Setting setting)
    {
        ArgumentNullException.ThrowIfNull(setting);

        return new SettingDto(setting.Id, setting.Key, setting.Value, setting.Description);
    }
}

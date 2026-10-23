using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Domain.Settings;

namespace Innovayse.Hostpanel.Application.Admin.Extensions;

/// <summary>
/// Maps a <see cref="Setting"/> to <see cref="SettingDto"/>. The single-setting and the list reads
/// both go through this one mapping, so a field added to the DTO is added once and a secret is
/// withheld in one place.
/// </summary>
public static class SettingMappingExtensions
{
    /// <summary>Maps a setting to its DTO, leaving out the value of a secret.</summary>
    /// <param name="setting">The setting.</param>
    /// <param name="secrecy">Decides whether the setting's value may leave the server.</param>
    /// <returns>The mapped DTO.</returns>
    public static SettingDto ToDto(this Setting setting, SettingSecrecy secrecy)
    {
        ArgumentNullException.ThrowIfNull(setting);
        ArgumentNullException.ThrowIfNull(secrecy);

        var isSecret = secrecy.IsSecret(setting.Key);
        var hasValue = !string.IsNullOrWhiteSpace(setting.Value);

        return new SettingDto(
            setting.Id,
            setting.Key,
            isSecret ? null : setting.Value,
            setting.Description,
            isSecret,
            hasValue);
    }
}

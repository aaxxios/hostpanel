using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Integrations.Common;

/// <summary>
/// Single-value reads of stored integration settings. A use case that needs one or two known
/// fields of one integration (a host, a port, a key) reads them here instead of repeating the
/// key-build-and-query pair at each call.
/// </summary>
public static class IntegrationSettingQueries
{
    /// <summary>
    /// Reads the plain value of one integration field, without change tracking. A secret field is
    /// stored encrypted and is decrypted here, so a caller always receives what the operator entered.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="cipher">Decrypts a stored secret.</param>
    /// <param name="slug">The integration's plugin id / slug.</param>
    /// <param name="field">The field name, for example <see cref="IntegrationSettingKeys.HostField"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The stored value, or <c>null</c> when no setting exists for the field.</returns>
    public static async Task<string?> GetIntegrationValueAsync(
        this IHostpanelDbContext context, ISecretSettingCipher cipher, string slug, string field, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(cipher);

        var key = IntegrationSettingKeys.FieldKey(slug, field);
        var setting = await context.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key, ct);
        return setting is null ? null : cipher.Reveal(setting.Value);
    }
}

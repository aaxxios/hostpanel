using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Queries.GetPublicSettings;

/// <summary>
/// Handles <see cref="GetPublicSettingsQuery"/> by returning only the settings the
/// public storefront needs.
/// <para>
/// The keys come from <see cref="PortalSettingKeys.Public"/> — an allow-list rather
/// than a prefix match on purpose. A prefix would silently start exposing anything a
/// future contributor happens to name <c>portal.*</c>, and this response is served
/// without authentication from a table that also holds integration credentials.
/// </para>
/// </summary>
public sealed class GetPublicSettingsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetPublicSettingsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns the allow-listed settings that currently have a value.
    /// </summary>
    /// <param name="query">The get public settings query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the exposed key/value pairs; empty when none are set. This use case refuses
    /// nothing — it is read anonymously by the storefront.
    /// </returns>
    public async Task<Result<IReadOnlyList<PublicSettingDto>>> HandleAsync(
        GetPublicSettingsQuery query,
        CancellationToken ct)
    {
        // Was ISettingRepository.ListAsync -- read-only, so no tracking is needed.
        var settings = await _context.Settings.AsNoTracking().OrderBy(s => s.Key).ToListAsync(ct);

        IReadOnlyList<PublicSettingDto> dtos = settings
            .Where(s => PortalSettingKeys.Public.Contains(s.Key) && !string.IsNullOrWhiteSpace(s.Value))
            .Select(s => new PublicSettingDto(s.Key, s.Value))
            .ToList();

        return Result<IReadOnlyList<PublicSettingDto>>.Success(dtos);
    }
}

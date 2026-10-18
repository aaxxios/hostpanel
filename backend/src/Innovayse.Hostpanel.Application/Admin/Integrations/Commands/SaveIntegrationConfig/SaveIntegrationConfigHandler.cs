using Innovayse.Hostpanel.Application.Admin.Integrations.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Innovayse.Hostpanel.Application.Admin.Integrations.Commands.SaveIntegrationConfig;

/// <summary>
/// Handles <see cref="SaveIntegrationConfigCommand"/> by upserting each field as a key-value
/// <see cref="Setting"/> and persisting in one transaction.
/// </summary>
public sealed class SaveIntegrationConfigHandler
{
    /// <summary>
    /// Placeholder value sent by the admin panel when a secret field was not changed. Receiving
    /// this value means "do not overwrite the stored secret."
    /// </summary>
    private const string MaskPlaceholder = "••••••••";

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    /// Invalidates the cached hosting-panel server-info results a saved credential change would make
    /// stale.
    /// </summary>
    private readonly IMemoryCache _cache;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="cache">
    /// Invalidates the cached hosting-panel server-info results a saved credential change would make
    /// stale.
    /// </param>
    public SaveIntegrationConfigHandler(IHostpanelDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    /// <summary>
    /// Upserts the is_enabled flag and every supplied field for the given slug, then saves all
    /// changes in a single call.
    /// </summary>
    /// <param name="command">The save integration config command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success. Always succeeds once validation has passed.</returns>
    public async Task<Result> HandleAsync(SaveIntegrationConfigCommand command, CancellationToken ct)
    {
        // The validator refuses an empty slug before the handler runs; the member is nullable only so
        // the route overlay can fill it (see the command), so narrow it once here.
        var slug = command.Slug ?? throw new InvalidOperationException("The slug is validated as required.");

        // Upsert the is_enabled flag.
        await UpsertAsync(
            IntegrationSettingKeys.EnabledKey(slug),
            command.IsEnabled ? IntegrationSettingKeys.EnabledValue : IntegrationSettingKeys.DisabledValue,
            $"Whether the {slug} integration is enabled.",
            ct);

        // Upsert each supplied field, skipping masked placeholders.
        foreach (var (fieldKey, fieldValue) in command.Config)
        {
            if (fieldValue == MaskPlaceholder)
            {
                continue; // Admin panel sent back a masked value -- do not overwrite stored secret.
            }

            await UpsertAsync(
                IntegrationSettingKeys.FieldKey(slug, fieldKey),
                fieldValue,
                $"{slug} integration field: {fieldKey}.",
                ct);
        }

        await _context.SaveChangesAsync(ct);

        // A saved hosting-panel credential invalidates the cached live server-info result the
        // read side serves for 5 minutes, so the operator sees the new credentials take effect
        // immediately rather than reading the old server's status until the cache expires.
        if (string.Equals(slug, IntegrationSlugs.Cwp, StringComparison.OrdinalIgnoreCase))
        {
            _cache.Remove(HostingPanelCacheKeys.Cwp);
        }

        if (string.Equals(slug, IntegrationSlugs.Cwp7, StringComparison.OrdinalIgnoreCase))
        {
            _cache.Remove(HostingPanelCacheKeys.Cwp7);
        }

        return Result.Success();
    }

    /// <summary>
    /// Finds an existing setting by <paramref name="key"/> and updates its value, or creates a new
    /// setting when none exists.
    /// </summary>
    /// <param name="key">The full setting key, e.g. <c>integration:{slug}:secret_key</c>.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="description">Human-readable description stored when creating a new setting.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous upsert operation.</returns>
    private async Task UpsertAsync(string key, string value, string description, CancellationToken ct)
    {
        var existing = await _context.Settings.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (existing is null)
        {
            _context.Settings.Add(Setting.Create(key, value, description));
        }
        else
        {
            existing.UpdateValue(value);
        }
    }
}

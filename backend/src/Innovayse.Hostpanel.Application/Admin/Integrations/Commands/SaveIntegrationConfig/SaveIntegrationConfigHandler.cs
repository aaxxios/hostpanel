using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Application.Admin.Common.Enums;
using Innovayse.Hostpanel.Application.Admin.Integrations.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Admin.Integrations.Commands.SaveIntegrationConfig;

/// <summary>
/// Handles <see cref="SaveIntegrationConfigCommand"/> by upserting each field as a key-value
/// <see cref="Setting"/> and persisting in one transaction.
/// </summary>
public sealed class SaveIntegrationConfigHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    /// Invalidates the cached hosting-panel server-info results a saved credential change would make
    /// stale.
    /// </summary>
    private readonly IMemoryCache _cache;

    /// <summary>Loaded plugins, whose manifests declare which of their fields are secret.</summary>
    private readonly IPluginRegistry _plugins;

    /// <summary>The acting admin, named in the audit line of a secret change.</summary>
    private readonly ICurrentUserService _currentUser;

    /// <summary>Receives the audit line of a secret change (key only, never the value).</summary>
    private readonly ILogger<SaveIntegrationConfigHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="cache">
    /// Invalidates the cached hosting-panel server-info results a saved credential change would make
    /// stale.
    /// </param>
    /// <param name="plugins">Loaded plugins, whose manifests declare which of their fields are secret.</param>
    /// <param name="currentUser">The acting admin, named in the audit line of a secret change.</param>
    /// <param name="logger">Receives the audit line of a secret change.</param>
    public SaveIntegrationConfigHandler(
        IHostpanelDbContext context,
        IMemoryCache cache,
        IPluginRegistry plugins,
        ICurrentUserService currentUser,
        ILogger<SaveIntegrationConfigHandler> logger)
    {
        _context = context;
        _cache = cache;
        _plugins = plugins;
        _currentUser = currentUser;
        _logger = logger;
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

        var secrecy = SettingSecrecy.From(_plugins);
        var clear = new HashSet<string>(command.ClearFields ?? [], StringComparer.OrdinalIgnoreCase);
        var audit = new List<(string Key, SecretSettingChange Change)>();

        foreach (var (fieldKey, fieldValue) in command.Config)
        {
            if (clear.Contains(fieldKey))
            {
                continue; // Cleared below; a clear wins over a value sent alongside it.
            }

            var key = IntegrationSettingKeys.FieldKey(slug, fieldKey);
            var isSecret = secrecy.IsSecret(key);

            // The admin panel never receives a secret, so an empty secret field means "unchanged".
            if (isSecret && string.IsNullOrWhiteSpace(fieldValue))
            {
                continue;
            }

            await UpsertAsync(key, fieldValue ?? string.Empty, $"{slug} integration field: {fieldKey}.", ct);
            if (isSecret)
            {
                audit.Add((key, SecretSettingChange.Replaced));
            }
        }

        foreach (var fieldKey in clear)
        {
            var key = IntegrationSettingKeys.FieldKey(slug, fieldKey);
            var existing = await _context.Settings.FirstOrDefaultAsync(s => s.Key == key, ct);
            if (existing is null || existing.Value.Length == 0)
            {
                continue;
            }

            existing.UpdateValue(string.Empty);
            if (secrecy.IsSecret(key))
            {
                audit.Add((key, SecretSettingChange.Cleared));
            }
        }

        await _context.SaveChangesAsync(ct);

        foreach (var (key, change) in audit)
        {
            SecretSettingAudit.Record(_logger, key, change, _currentUser.UserId);
        }

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

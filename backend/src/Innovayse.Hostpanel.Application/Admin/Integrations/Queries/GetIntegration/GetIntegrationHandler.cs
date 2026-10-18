using Innovayse.Hostpanel.Application.Admin.Integrations.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Admin.Integrations.Queries.GetIntegration;

/// <summary>
/// Handles <see cref="GetIntegrationQuery"/> by loading all settings for the given slug and
/// returning them with secret values masked alongside field definitions.
/// </summary>
public sealed class GetIntegrationHandler
{
    /// <summary>
    /// Substring patterns that identify a field as a secret. Any field key containing one of these
    /// strings (case-insensitive) will be masked.
    /// </summary>
    private static readonly string[] SecretMarkers = ["key", "secret", "password", "token"];

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Plugin registry for resolving plugin-provided integrations.</summary>
    private readonly IPluginRegistry _plugins;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="plugins">Plugin registry for resolving plugin-provided integrations.</param>
    public GetIntegrationHandler(IHostpanelDbContext context, IPluginRegistry plugins)
    {
        _context = context;
        _plugins = plugins;
    }

    /// <summary>
    /// Loads the configuration for the requested integration or plugin, masks secret fields, and
    /// returns field definitions for dynamic form rendering.
    /// </summary>
    /// <param name="query">Query containing the integration slug.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The integration detail with masked config and field definitions, or
    /// <c>INTEGRATION_NOT_FOUND</c> when the slug names neither a built-in integration nor a
    /// loaded plugin.
    /// </returns>
    public async Task<Result<IntegrationDetailDto>> HandleAsync(GetIntegrationQuery query, CancellationToken ct)
    {
        IReadOnlyList<FieldDefinitionDto> fields;

        if (IntegrationCatalogue.TryGet(query.Slug, out var meta))
        {
            fields = meta.Fields;
        }
        else
        {
            var manifest = _plugins.GetLoadedManifests()
                .FirstOrDefault(m => m.Id == query.Slug);

            if (manifest is null)
            {
                return Result<IntegrationDetailDto>.FailureCode(HostpanelErrorCodes.INTEGRATION_NOT_FOUND);
            }

            fields = manifest.Fields
                .Select(f => new FieldDefinitionDto(f.Key, f.Label, f.Type, f.Required))
                .ToList();
        }

        var prefix = IntegrationSettingKeys.Prefix(query.Slug);
        var settings = await IntegrationSettingsLookup.LoadAsync(_context, prefix, ct);
        var isEnabled = settings.IsEnabled(query.Slug);

        var config = new Dictionary<string, string>(fields.Count);
        foreach (var field in fields)
        {
            config[field.Key] = MaskIfSecret(field.Key, settings.GetValue(query.Slug, field.Key) ?? string.Empty);
        }

        return Result<IntegrationDetailDto>.Success(new IntegrationDetailDto(query.Slug, isEnabled, config, fields));
    }

    /// <summary>
    /// Returns "••••••••" when <paramref name="fieldKey"/> is a secret field and
    /// <paramref name="value"/> is non-empty; otherwise returns <paramref name="value"/> unchanged.
    /// </summary>
    /// <param name="fieldKey">The field key to inspect.</param>
    /// <param name="value">The raw stored value.</param>
    /// <returns>The original value or the masked placeholder.</returns>
    private static string MaskIfSecret(string fieldKey, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var isSecret = SecretMarkers.Any(m => fieldKey.Contains(m, StringComparison.OrdinalIgnoreCase));
        return isSecret ? "••••••••" : value;
    }
}

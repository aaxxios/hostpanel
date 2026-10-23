using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Application.Admin.Integrations.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Admin.Integrations.Queries.GetIntegration;

/// <summary>
/// Handles <see cref="GetIntegrationQuery"/> by loading all settings for the given slug and
/// returning them alongside field definitions, with secret values withheld.
/// </summary>
public sealed class GetIntegrationHandler
{
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
    /// Loads the configuration for the requested integration or plugin, withholds secret values, and
    /// returns field definitions for dynamic form rendering.
    /// </summary>
    /// <param name="query">Query containing the integration slug.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The integration detail with its field states and definitions, or
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

        var secrecy = SettingSecrecy.From(_plugins);
        var config = new Dictionary<string, IntegrationFieldValueDto>(fields.Count);
        foreach (var field in fields)
        {
            var key = IntegrationSettingKeys.FieldKey(query.Slug, field.Key);
            var value = settings.GetValue(query.Slug, field.Key);
            var hasValue = !string.IsNullOrWhiteSpace(value);

            // A secret is answered with whether it is set, never with its value or a mask of it.
            config[field.Key] = secrecy.IsSecret(key)
                ? new IntegrationFieldValueDto(null, IsSecret: true, hasValue)
                : new IntegrationFieldValueDto(value ?? string.Empty, IsSecret: false, hasValue);
        }

        return Result<IntegrationDetailDto>.Success(new IntegrationDetailDto(query.Slug, isEnabled, config, fields));
    }
}

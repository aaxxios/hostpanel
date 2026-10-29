using Innovayse.Hostpanel.Application.Admin.Integrations.Common;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.SDK.Plugins;

namespace Innovayse.Hostpanel.Application.Admin.Integrations.Extensions;

/// <summary>
/// Maps a field a plugin manifest declares to the <see cref="FieldDefinitionDto"/> the admin form
/// renders. The plugin loader and the integration detail read share it, so a manifest field is
/// read the same way wherever it is shown.
/// </summary>
public static class PluginFieldMappingExtensions
{
    /// <summary>Maps one manifest field.</summary>
    /// <param name="field">The field from <c>plugin.json</c>.</param>
    /// <returns>The form definition, its type read with <see cref="IntegrationFieldTypes.FromManifest"/>.</returns>
    public static FieldDefinitionDto ToFieldDefinition(this PluginField field)
    {
        ArgumentNullException.ThrowIfNull(field);

        return new FieldDefinitionDto(field.Key, field.Label, IntegrationFieldTypes.FromManifest(field.Type), field.Required);
    }
}

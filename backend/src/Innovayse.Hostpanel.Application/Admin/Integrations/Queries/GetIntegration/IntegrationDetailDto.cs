using Innovayse.Hostpanel.Application.Admin.Integrations.Common;

namespace Innovayse.Hostpanel.Application.Admin.Integrations.Queries.GetIntegration;
/// <summary>
/// Full configuration for one integration. A secret field never carries its value.
/// </summary>
/// <param name="Slug">URL-safe identifier for the integration.</param>
/// <param name="IsEnabled">Whether the integration is currently active.</param>
/// <param name="Config">
/// Field key (e.g. "secret_key") to its stored state. A secret field, as classified by
/// <c>SettingSecrecy</c>, answers only <c>isSecret</c> and <c>hasValue</c>; its value is <c>null</c>.
/// </param>
/// <param name="FieldDefinitions">
/// Ordered list of field metadata used by the admin panel to render the config form.
/// </param>
public record IntegrationDetailDto(
    string Slug,
    bool IsEnabled,
    IReadOnlyDictionary<string, IntegrationFieldValueDto> Config,
    IReadOnlyList<FieldDefinitionDto> FieldDefinitions);

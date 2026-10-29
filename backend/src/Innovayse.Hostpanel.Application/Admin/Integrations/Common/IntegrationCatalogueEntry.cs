namespace Innovayse.Hostpanel.Application.Admin.Integrations.Common;

/// <summary>Static description of one built-in integration.</summary>
/// <param name="Name">Display name shown in the integrations grid.</param>
/// <param name="Category">Category label the grid groups the integration under.</param>
/// <param name="Fields">Ordered field definitions the admin form renders.</param>
/// <param name="RequiredFields">
/// Field keys that must hold a non-blank stored value for the integration to count as configured.
/// Kept separate from the form's <see cref="FieldDefinitionDto.Required"/> flags on purpose: the two
/// have always differed for a few integrations (for example the form marks the card gateway's publishable key
/// as required while configuration only needs the secret key), and this catalogue preserves both.
/// </param>
public sealed record IntegrationCatalogueEntry(
    string Name,
    string Category,
    IReadOnlyList<FieldDefinitionDto> Fields,
    IReadOnlyList<string> RequiredFields);

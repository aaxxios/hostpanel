using Innovayse.Hostpanel.Domain.Settings.Enums;

namespace Innovayse.Hostpanel.Domain.Settings;

/// <summary>
/// Reads the <c>type</c> a plugin manifest gives a field into <see cref="IntegrationFieldType"/>.
/// A manifest is third-party JSON, so the name is matched without regard to case and a kind this
/// release does not know is rendered as plain text rather than refused.
/// </summary>
public static class IntegrationFieldTypes
{
    /// <summary>Parses a manifest field type.</summary>
    /// <param name="manifestType">The <c>type</c> string from <c>plugin.json</c>, e.g. <c>secret</c>.</param>
    /// <returns>The matching kind, or <see cref="IntegrationFieldType.Text"/> when none matches.</returns>
    public static IntegrationFieldType FromManifest(string? manifestType)
    {
        return Enum.TryParse<IntegrationFieldType>(manifestType, ignoreCase: true, out var type)
            && Enum.IsDefined(type)
            ? type
            : IntegrationFieldType.Text;
    }
}

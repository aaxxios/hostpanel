namespace Innovayse.Hostpanel.Domain.Settings.Enums;

/// <summary>
/// The templates the storefront can render. The stored <c>portal.template</c> value is the member
/// name in lower case (<see cref="PortalSettingKeys.TemplateValue"/>); the list mirrors
/// <c>client/types/templatename.ts</c>.
/// </summary>
public enum StorefrontTemplate
{
    /// <summary>The original storefront design, light and dark; the default on a fresh install.</summary>
    Classic = 0,

    /// <summary>The glow-backed design with Armenian typography.</summary>
    Aurora = 1,

    /// <summary>The editorial design with a teal palette.</summary>
    Nova = 2,
}

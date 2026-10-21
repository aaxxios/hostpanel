/**
 * The closed value sets of the portal-appearance settings: which template the storefront renders
 * and which colour mode a visitor starts in. The members are the strings the API accepts.
 */

/** The templates the portal ships. Mirrors `client/templates/types.ts` and `PortalSettingKeys.TemplateNames`. */
export enum PortalTemplate {
  /** Dark and light, Armenian typography. */
  Aurora = 'aurora',
  /** Editorial layout with a teal palette. */
  Nova = 'nova',
  /** The original storefront design, light and dark; the default on a fresh install. */
  Classic = 'classic',
}

/** The colour modes an operator can set as the default. Mirrors `PortalSettingKeys.ThemeModes`. */
export enum PortalThemeMode {
  /** Always light. */
  Light = 'light',
  /** Always dark. */
  Dark = 'dark',
  /** Follows the visitor's device. */
  System = 'system',
}

/**
 * Every template the portal can render; the values are what the `portal.template` setting holds.
 *
 * @module types/templatename
 */
export enum TemplateName {
  /** The glow-backed design, light and dark. */
  Aurora = 'aurora',
  /** The second design. */
  Nova = 'nova',
  /** The original storefront design, light and dark; the default (`DEFAULT_TEMPLATE`). */
  Classic = 'classic'
}

/**
 * A capability card: icon, title and body line.
 *
 * @module templates/nova/types/feature
 */

/**
 * A card that states one capability: an icon, a title and a body line.
 *
 * Used by the "why", performance, security and trust sections, which differ in
 * layout but not in the shape of what they render.
 */
export interface Feature {
  /** Stable key, used for the list key. */
  key: string
  /** Iconify name, e.g. `lucide:shield-check`. */
  icon: string
  /** i18n key for the title. */
  titleKey: string
  /** i18n key for the body; omitted where the section shows titles only. */
  bodyKey?: string
  /**
   * Operator setting that must be non-empty for this card to render, by its
   * `portal.*` setting key, read through {@link usePortalSettings} from settings only (no env fallback).
   *
   * Present on every claim this codebase cannot verify on its own: NVMe
   * storage, a CDN, a malware scanner. The platform provisions accounts through
   * a hosting control panel and has no idea what hardware sits behind it, so
   * printing those as facts would be inventing them. An operator who does offer
   * them turns the line on from Admin → Settings; everyone else never sees it.
   */
  requiresSetting?: string
}

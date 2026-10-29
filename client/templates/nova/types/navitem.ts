/**
 * One navigation entry of nova's menus.
 *
 * @module templates/nova/types/navitem
 */

/**
 * One navigation entry.
 *
 * `to` is a path relative to the locale root; the components run it through
 * `useLocalePath`. Every entry must point at a route this portal actually has:
 * a link to a page that does not exist is a 404 the visitor blames on the host.
 */
export interface NavItem {
  /** Stable key, used for the list key and to mark the current entry. */
  key: string
  /** i18n key for the visible label. */
  labelKey: string
  /** Target path, pre-locale. */
  to: string
}

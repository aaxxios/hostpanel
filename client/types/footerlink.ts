/**
 * One link of a classic footer column.
 *
 * @module types/footerlink
 */

/** One footer link: a stable key for the list and test ids, the visible label and an in-app route. */
export interface FooterLink {
  /** Stable key, unique within its column; also the tail of the link's test id. */
  key: string
  /** Visible, already translated label. */
  label: string
  /** In-app route, already prefixed with the active locale. */
  to: string
}

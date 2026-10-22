/**
 * One link inside a header dropdown or a drawer section.
 *
 * @module types/headernavchild
 */

/** A member of a header navigation group. */
export interface HeaderNavChild {
  /** Stable key, used for list keys and test ids. */
  key: string
  /** Visible label. */
  label: string
  /** Short description shown under the label; absent when the link needs none. */
  desc?: string
  /** Localised route the link goes to. */
  to: string
}

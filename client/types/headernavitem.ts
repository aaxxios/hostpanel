/**
 * One top-level entry of the classic header navigation.
 *
 * @module types/headernavitem
 */

import type { HeaderNavChild } from '~/types/headernavchild'

/** A top-level header entry: a plain link, or a group when it has `children`. */
export interface HeaderNavItem {
  /** Stable key, used for list keys and test ids. */
  key: string
  /** Visible label. */
  label: string
  /** Localised route; absent when the entry is a group. */
  to?: string
  /** Group members; absent for a plain link. */
  children?: HeaderNavChild[]
}

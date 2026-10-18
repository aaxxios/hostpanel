/**
 * One entry of the site header navigation.
 *
 * An entry with `children` renders as a dropdown on desktop and an expandable group on mobile;
 * one without is a plain link.
 *
 * @module types/navlink
 */

import type { NavChildLink } from '~/types/navchildlink'

/** A top-level navigation entry. */
export interface NavLink {
  /** Visible label. */
  label: string
  /** Localised route; absent when the entry is a group. */
  href?: string
  /** Dropdown / group members; absent for a plain link. */
  children?: NavChildLink[]
}

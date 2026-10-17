/**
 * A link inside a dropdown group.
 *
 * @module types/navchildlink
 */

import type { NavIcon } from '~/types/navicon'

/** A link inside a dropdown group. */
export interface NavChildLink {
  /** Localised route the link goes to. */
  href: string
  /** Visible label. */
  label: string
  /** Short description shown under the label on desktop. */
  desc?: string
  /** Icon shown before the label. */
  icon?: NavIcon
}

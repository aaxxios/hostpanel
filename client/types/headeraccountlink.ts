/**
 * One destination of the signed-in account menu.
 *
 * @module types/headeraccountlink
 */

import type { Component } from 'vue'

/** A client-area link offered by the header account menu and the mobile drawer. */
export interface HeaderAccountLink {
  /** Stable key, used for list keys and test ids. */
  key: string
  /** Visible label. */
  label: string
  /** Localised route. */
  to: string
  /** Glyph shown before the label. */
  icon: Component
}

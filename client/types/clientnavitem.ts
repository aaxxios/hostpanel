/**
 * One entry of the client area's sidebar navigation.
 *
 * @module types/clientnavitem
 */

import type { Component } from 'vue'

/** A sidebar link: where it goes, what it says and the icon in front of it. */
export interface ClientNavItem {
  /** Route the link goes to; also decides whether it is the active one. */
  to: string
  /** Visible, already-translated text. */
  label: string
  /** Lucide icon component drawn before the label. */
  icon: Component
}

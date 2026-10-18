/**
 * Types for `UiSidebar`.
 */
import type { RouteLocationRaw } from 'vue-router'

/** One entry of a `UiSidebar`. */
export interface UiSidebarItem {
  /** Stable key; matched against the sidebar's `activeKey` and used in the entry's test id. */
  key: string
  /** Caption shown when the sidebar is expanded (already translated). */
  label: string
  /** Where the entry navigates. */
  to: RouteLocationRaw
  /** Inner SVG markup of the entry's icon (shape tags only; rendered without `v-html`). */
  icon: string
}

/**
 * Drawer vocabulary for `UiDrawer`.
 */

/** Closed set of edges the drawer attaches to. */
export const UiDrawerSide = {
  /** The `left` option. */
  Left: 'left',
  /** The `right` option. */
  Right: 'right',
} as const

/** One of the UiDrawerSide values. */
export type UiDrawerSide = (typeof UiDrawerSide)[keyof typeof UiDrawerSide]

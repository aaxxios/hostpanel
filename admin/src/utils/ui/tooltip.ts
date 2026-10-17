/**
 * Placement vocabulary for `UiTooltip`.
 */

/** Closed set of placements relative to the trigger. */
export const UiTooltipPlacement = {
  /** The `top` option. */
  Top: 'top',
  /** The `bottom` option. */
  Bottom: 'bottom',
} as const

/** One of the UiTooltipPlacement values. */
export type UiTooltipPlacement = (typeof UiTooltipPlacement)[keyof typeof UiTooltipPlacement]

/**
 * Types for `UiTabs`.
 */

/** One tab of a `UiTabs` strip. */
export interface UiTabItem {
  /** Value the tab stands for; becomes the model when selected. */
  value: string
  /** Visible caption (already translated). */
  label: string
  /** Blocks selecting the tab. */
  disabled?: boolean
  /** `data-testid` of the tab button. */
  testid?: string
}

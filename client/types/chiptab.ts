/**
 * One chip of a `UiChipTabs` filter.
 *
 * @module types/chiptab
 */

/** One chip: the value it selects, the label it shows, an optional count and its e2e id. */
export interface ChipTab<T extends string = string> {
  /** The value the filter takes when this chip is chosen. */
  value: T
  /** Visible, already translated label. */
  label: string
  /** Optional count shown after the label (how many items the chip would show). */
  count?: number
  /** `data-testid` of the chip (`<area>-<element>`). */
  testid?: string
}

/**
 * One choice of a `UiSegmentedControl`.
 *
 * @module types/segmentedoption
 */
import type { SegmentedValue } from '~/types/segmentedvalue'

/** One segment: the value it selects, the label it shows and its e2e id. */
export interface SegmentedOption<T extends SegmentedValue = SegmentedValue> {
  /** The value the control takes when this segment is chosen. */
  value: T
  /** Visible, already translated label. */
  label: string
  /** `data-testid` of the segment (`<area>-<element>`). */
  testid: string
}

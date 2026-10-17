/**
 * Tone of a support rating, shared by the ticket feedback reports.
 */
import { UiTone } from '@/utils/ui/tone'

/** Rating from which feedback counts as good. */
const GOOD_RATING = 8

/** Rating from which feedback counts as acceptable. */
const FAIR_RATING = 5

/**
 * Picks the badge tone of a rating from 1 to 10.
 *
 * @param rating - The rating.
 * @returns Success for a good rating, warning for a fair one, danger below that.
 */
export const ratingTone = (rating: number): UiTone => {
  if (rating >= GOOD_RATING) {
    return UiTone.Success
  }
  return rating >= FAIR_RATING ? UiTone.Warning : UiTone.Danger
}

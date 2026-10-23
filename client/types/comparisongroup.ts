/**
 * One comparable product group of the products-page comparison table.
 *
 * @module types/comparisongroup
 */

import type { ComparisonPlan } from '~/types/comparisonplan'
import type { ComparisonRow } from '~/types/comparisonrow'

/** A catalogue group whose plans share enough specification lines to be compared. */
export interface ComparisonGroup {
  /** Catalogue group id. */
  id: number
  /** Group display name, the label of its tab. */
  name: string
  /** The plans, one column each, in display order. */
  plans: ComparisonPlan[]
  /** Specification rows aligned to {@link plans}. */
  rows: ComparisonRow[]
}

/**
 * How a comparison row renders its cells.
 *
 * @module types/comparisonvaluekind
 */

/** How a comparison row renders its cells. */
export enum ComparisonValueKind {
  /** The cell shows text taken from the plan's data. */
  Dynamic = 'dynamic',
  /** The cell shows a tick or a dash. */
  Check = 'check',
}

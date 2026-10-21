/**
 * Column logic for grids of cards whose count the operator controls.
 *
 * A fixed `grid-cols-3` leaves an orphan on the last row whenever the count is not a multiple of
 * three. The helpers here pick the column count from the item count, so the last row is as full as
 * the count allows, and the grid component centres whatever row is still incomplete.
 *
 * @module utils/balancedGrid
 */

/** The widest a row of cards gets on a desktop screen. */
export const BALANCED_MAX_COLUMNS = 4

/** The narrowest "wide" row; below this many items the row simply holds all of them. */
export const BALANCED_MIN_COLUMNS = 3

/** The most columns a tablet-width screen shows. */
export const BALANCED_TABLET_COLUMNS = 2

/**
 * The column count a grid of `count` items uses on a desktop screen.
 *
 * Up to four items share one row. Beyond that the count that leaves the fullest last row wins
 * (5 → 3+2, 6 → 3+3, 7 → 4+3, 8 → 4+4, 9 → 3x3, 10 → 4+4+2); a tie goes to the narrower row.
 *
 * @param count Number of items; zero or less is treated as one so a CSS variable never reads 0.
 * @returns A column count between 1 and {@link BALANCED_MAX_COLUMNS}.
 */
export const balancedColumns = (count: number): number => {
  const items = Math.max(1, Math.floor(count))
  if (items <= BALANCED_MAX_COLUMNS) {
    return items
  }
  let best = BALANCED_MIN_COLUMNS
  let bestFill = items % best || best
  for (let columns = BALANCED_MIN_COLUMNS + 1; columns <= BALANCED_MAX_COLUMNS; columns++) {
    const fill = items % columns || columns
    if (fill * best > bestFill * columns) {
      best = columns
      bestFill = fill
    }
  }
  return best
}

/**
 * The column count used on a tablet-width screen.
 *
 * @param count Number of items.
 * @returns At most {@link BALANCED_TABLET_COLUMNS}, and never more than the items there are.
 */
export const tabletColumns = (count: number): number =>
  Math.min(BALANCED_TABLET_COLUMNS, Math.max(1, Math.floor(count)))

/**
 * How many items sit on the last row.
 *
 * @param count Number of items.
 * @param columns Columns per row.
 * @returns The size of the last row, a full row when the count divides evenly.
 */
export const lastRowSize = (count: number, columns: number): number =>
  count <= 0 ? 0 : (count % columns || columns)

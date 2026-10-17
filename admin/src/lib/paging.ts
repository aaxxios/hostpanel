/**
 * Page arithmetic shared by every paged list: how many pages a total makes, which slice of an
 * in-memory list a page shows, and the first and last row number of a page for the
 * "showing x to y" line.
 */

/**
 * Counts the pages a list of `total` rows makes; an empty list still has one (empty) page.
 *
 * @param total - Number of rows across all pages.
 * @param pageSize - Rows per page.
 * @returns The page count, at least 1.
 */
export const pageCount = (total: number, pageSize: number): number => Math.max(1, Math.ceil(total / pageSize))

/**
 * Cuts one page out of an in-memory list.
 *
 * @param items - The whole list.
 * @param page - Page number, starting at 1.
 * @param pageSize - Rows per page.
 * @returns The rows of that page (fewer on the last page, none past it).
 */
export const pageSlice = <T>(items: readonly T[], page: number, pageSize: number): T[] =>
  items.slice((page - 1) * pageSize, page * pageSize)

/**
 * Number of the first row on a page, for the record count line.
 *
 * @param page - Page number, starting at 1.
 * @param pageSize - Rows per page.
 * @returns The 1-based row number.
 */
export const pageFirstRow = (page: number, pageSize: number): number => (page - 1) * pageSize + 1

/**
 * Number of the last row on a page, for the record count line.
 *
 * @param page - Page number, starting at 1.
 * @param pageSize - Rows per page.
 * @param total - Number of rows across all pages.
 * @returns The 1-based row number, never past `total`.
 */
export const pageLastRow = (page: number, pageSize: number, total: number): number => Math.min(page * pageSize, total)

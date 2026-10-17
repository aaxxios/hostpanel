/**
 * Indexed access that fails loudly. Under `noUncheckedIndexedAccess` an array element is
 * `T | undefined`; a test that reads the n-th wrapper wants the wrapper or a clear failure, not a
 * non-null assertion that hides a mistyped index.
 *
 * @module tests/helpers/at
 */

/**
 * Returns the item at an index, or throws when the list is shorter.
 *
 * @param items - The list to read from.
 * @param index - Zero-based position.
 * @returns The item at that position.
 */
export const at = <T>(items: readonly T[], index: number): T => {
  const item = items[index]
  if (item === undefined) {
    throw new Error(`Expected an item at index ${index}, but the list has ${items.length}.`)
  }
  return item
}

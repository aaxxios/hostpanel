/**
 * Reads one period's figure out of a TLD action's price list.
 *
 * @module utils/tldPeriodPrice
 */

import type { TldPeriodPrice } from '~/types/tldperiodprice'

/** The period the storefront quotes and adds to the cart by default. */
export const ONE_YEAR = 1

/**
 * The price of an action for a given period.
 *
 * A zero or negative figure is how a registrar marks a period it does not offer, so it reads
 * the same as a missing one.
 *
 * @param prices - The action's price list, or undefined when the extension has none.
 * @param years - The period in whole years.
 * @returns The amount, or null when that period is not offered.
 */
export const priceForYears = (prices: TldPeriodPrice[] | undefined, years: number): number | null => {
  const amount = prices?.find((price) => price.years === years)?.amount
  return amount !== undefined && amount > 0 ? amount : null
}

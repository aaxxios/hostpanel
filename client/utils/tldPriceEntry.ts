/**
 * Finds one extension's entry in a TLD price table.
 *
 * @module utils/tldPriceEntry
 */

import type { TldPriceEntry } from '~/types/tldpriceentry'
import type { TldPricing } from '~/types/tldpricing'

/**
 * The entry of an extension.
 *
 * @param pricing - The price table, or null/undefined while it has not loaded.
 * @param tld - The extension without a leading dot, e.g. "com".
 * @returns The entry, or undefined when the extension is not offered.
 */
export const tldPriceEntry = (pricing: TldPricing | null | undefined, tld: string): TldPriceEntry | undefined =>
  pricing?.pricing.find(entry => entry.tld === tld)

/**
 * Unit tests for `utils/tldPriceEntry.ts`: an extension is found by its name in the price table,
 * and an unloaded table or an unlisted extension has no entry.
 *
 * @module tests/utils/tldPriceEntry
 */

import { describe, expect, it } from 'vitest'
import type { TldPricing } from '~/types/tldpricing'
import { tldPriceEntry } from '~/utils/tldPriceEntry'

/** A table of two extensions. */
const table: TldPricing = {
  currency: { code: 'USD', prefix: '$' },
  pricing: [
    { tld: 'com', sellCurrency: 'USD', register: [{ years: 1, amount: 10 }] },
    { tld: 'net', sellCurrency: 'USD', register: [{ years: 1, amount: 12 }] },
  ],
}

describe('tldPriceEntry', () => {
  it('finds an extension by its name', () => {
    expect(tldPriceEntry(table, 'net')?.register).toEqual([{ years: 1, amount: 12 }])
  })

  it('has no entry for an extension the table does not list', () => {
    expect(tldPriceEntry(table, 'org')).toBeUndefined()
  })

  it('has no entry while the table has not loaded', () => {
    expect(tldPriceEntry(null, 'com')).toBeUndefined()
    expect(tldPriceEntry(undefined, 'com')).toBeUndefined()
  })
})

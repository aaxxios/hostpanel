/**
 * Unit tests for `utils/tldPeriodPrice.ts`: a period's price is read by its year count, and a
 * missing, zero or negative figure is not offered.
 *
 * @module tests/utils/tldPeriodPrice
 */

import { describe, expect, it } from 'vitest'
import { ONE_YEAR, priceForYears } from '~/utils/tldPeriodPrice'

describe('priceForYears', () => {
  it('reads the price of the asked period', () => {
    const prices = [{ years: 1, amount: 9.99 }, { years: 2, amount: 18.5 }]

    expect(priceForYears(prices, ONE_YEAR)).toBe(9.99)
    expect(priceForYears(prices, 2)).toBe(18.5)
  })

  it('is null for a period that is not listed', () => {
    expect(priceForYears([{ years: 2, amount: 18.5 }], ONE_YEAR)).toBeNull()
  })

  it('is null when the extension has no list', () => {
    expect(priceForYears(undefined, ONE_YEAR)).toBeNull()
  })

  it('treats a zero or negative figure as not offered', () => {
    expect(priceForYears([{ years: 1, amount: 0 }], ONE_YEAR)).toBeNull()
    expect(priceForYears([{ years: 1, amount: -1 }], ONE_YEAR)).toBeNull()
  })
})

/**
 * Tests for the TLD price-book arithmetic: price maps, margins and the
 * price cell, shared by the TLD list and form.
 */
import { describe, expect, it } from 'vitest'
import {
  TLD_PERIODS,
  calcMargin,
  emptyPriceMap,
  formatMarginPercent,
  formatTldPrice,
  fromApiMap,
  marginPercentTone,
  marginTone,
  toNonZeroMap,
} from '@/lib/tldPricing'

describe('price maps', () => {
  it('emptyPriceMap has a zero for every listed period', () => {
    expect(Object.keys(emptyPriceMap())).toEqual([...TLD_PERIODS])
    expect(Object.values(emptyPriceMap()).every((value) => value === 0)).toBe(true)
  })

  it('emptyPriceMap returns a fresh object each time', () => {
    expect(emptyPriceMap()).not.toBe(emptyPriceMap())
  })

  it('fromApiMap fills the missing periods and keeps a period this build does not list', () => {
    const filled = fromApiMap({ '1': 9.5, '7': 30 })

    expect(filled['1']).toBe(9.5)
    expect(filled['2']).toBe(0)
    expect(filled['7']).toBe(30)
  })

  it('toNonZeroMap keeps only the periods that carry a price', () => {
    expect(toNonZeroMap({ '1': 9.5, '2': 0, '3': -1, '5': 20 })).toEqual({ '1': 9.5, '5': 20 })
  })
})

describe('margin', () => {
  it('works out a signed percentage from cost and sell', () => {
    expect(calcMargin(10, 12.5)).toBe('+25.0%')
    expect(calcMargin(10, 8)).toBe('-20.0%')
  })

  it('shows a dash when either price is missing or zero', () => {
    expect(calcMargin(undefined, 5)).toBe('—')
    expect(calcMargin(5, undefined)).toBe('—')
    expect(calcMargin(0, 5)).toBe('—')
  })

  it('tones the margin green for a profit, red for a loss and muted when unknown', () => {
    expect(marginTone(10, 12)).toBe('success')
    expect(marginTone(10, 8)).toBe('danger')
    expect(marginTone(10, undefined)).toBe('muted')
  })

  it('formats and tones the margin percentage the API sent', () => {
    expect(formatMarginPercent(12.34)).toBe('+12.3%')
    expect(formatMarginPercent(-3)).toBe('-3.0%')
    expect(formatMarginPercent(null)).toBe('—')
    expect(marginPercentTone(0)).toBe('success')
    expect(marginPercentTone(-1)).toBe('danger')
    expect(marginPercentTone(null)).toBe('muted')
  })
})

describe('price cell and currency', () => {
  it('prefixes the price with the currency symbol', () => {
    expect(formatTldPrice(12, 'USD')).toBe('$12.00')
    expect(formatTldPrice(7.5, 'EUR')).toBe('€7.50')
  })

  it('shows a dash for a missing price and no symbol for an unknown currency', () => {
    expect(formatTldPrice(null, 'USD')).toBe('—')
    expect(formatTldPrice(3, 'XXX')).toBe('3.00')
  })
})

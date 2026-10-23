/**
 * Unit tests for the yearly-saving helpers: the percent between two per-month prices and the saving
 * of a plan's annual price over twelve monthly ones.
 */
import { describe, expect, it } from 'vitest'
import { savingPercent, yearlySavingPercent } from '~/utils/yearlySaving'

describe('savingPercent', () => {
  it('rounds the saving to a whole percent', () => {
    expect(savingPercent(10, 8.5)).toBe(15)
    expect(savingPercent(3, 2)).toBe(33)
  })

  it('answers null when there is nothing to announce', () => {
    expect(savingPercent(10, 10)).toBeNull()
    expect(savingPercent(10, 12)).toBeNull()
    expect(savingPercent(0, 0)).toBeNull()
    expect(savingPercent(null, 5)).toBeNull()
    expect(savingPercent(5, undefined)).toBeNull()
  })
})

describe('yearlySavingPercent', () => {
  it('compares the annual price per month with the monthly price', () => {
    expect(yearlySavingPercent({ monthly: 10, annual: 100 })).toBe(17)
  })

  it('answers null when a price is missing or the year is no cheaper', () => {
    expect(yearlySavingPercent({ monthly: 10, annual: null })).toBeNull()
    expect(yearlySavingPercent({ monthly: null, annual: 100 })).toBeNull()
    expect(yearlySavingPercent({ monthly: 10, annual: 120 })).toBeNull()
    expect(yearlySavingPercent(null)).toBeNull()
    expect(yearlySavingPercent(undefined)).toBeNull()
  })
})

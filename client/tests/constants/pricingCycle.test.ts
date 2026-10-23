/**
 * Unit tests for the pricing-cycle constants: both cycles are offered, in order, and each one maps to
 * the spelling the locale files use.
 */
import { describe, expect, it } from 'vitest'
import { PRICING_CYCLES, PRICING_CYCLE_I18N_KEY, PRICING_CYCLE_ISO_DURATION, MONTHS_PER_YEAR } from '~/constants/pricingCycle'
import { PricingCycle } from '~/types/pricingcycle'

describe('pricing cycle constants', () => {
  it('offers monthly first, then annual', () => {
    expect(PRICING_CYCLES).toEqual([PricingCycle.Monthly, PricingCycle.Annual])
  })

  it('maps every cycle to its locale key, spelling the yearly one "annually"', () => {
    expect(PRICING_CYCLE_I18N_KEY[PricingCycle.Monthly]).toBe('monthly')
    expect(PRICING_CYCLE_I18N_KEY[PricingCycle.Annual]).toBe('annually')
    for (const cycle of Object.values(PricingCycle)) {
      expect(PRICING_CYCLE_I18N_KEY[cycle]).toBeTruthy()
    }
  })

  it('maps every cycle to the ISO duration one payment covers', () => {
    expect(PRICING_CYCLE_ISO_DURATION[PricingCycle.Monthly]).toBe('P1M')
    expect(PRICING_CYCLE_ISO_DURATION[PricingCycle.Annual]).toBe('P1Y')
  })

  it('counts twelve months in a year', () => {
    expect(MONTHS_PER_YEAR).toBe(12)
  })
})

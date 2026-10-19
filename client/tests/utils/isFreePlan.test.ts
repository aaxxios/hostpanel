/**
 * Unit tests for `utils/isFreePlan.ts`: free is a zero monthly price, never a name.
 *
 * @module tests/utils/isFreePlan
 */

import { describe, expect, it } from 'vitest'
import { isFreePlan } from '~/utils/isFreePlan'

describe('isFreePlan', () => {
  it('is true for a zero monthly price', () => {
    expect(isFreePlan({ monthly: 0, annual: 0 })).toBe(true)
  })

  it('is false for a priced plan', () => {
    expect(isFreePlan({ monthly: 5, annual: 50 })).toBe(false)
  })

  it('is false when no price is offered', () => {
    expect(isFreePlan({ monthly: null, annual: 0 })).toBe(false)
    expect(isFreePlan(null)).toBe(false)
    expect(isFreePlan(undefined)).toBe(false)
  })
})

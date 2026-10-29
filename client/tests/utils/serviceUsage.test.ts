/**
 * Tests for `utils/serviceUsage.ts`: what counts as a limit and how the percentage is capped.
 */
import { describe, expect, it } from 'vitest'
import { hasUsageLimit, usagePercent } from '~/utils/serviceUsage'

describe('hasUsageLimit', () => {
  it.each([
    [1024, true],
    [0, false],
    [-1, false]
  ])('reads %s as a limit: %s', (limit, expected) => {
    expect(hasUsageLimit(limit)).toBe(expected)
  })
})

describe('usagePercent', () => {
  it('gives the share used', () => {
    expect(usagePercent(256, 1024)).toBe(25)
  })

  it('caps at 100', () => {
    expect(usagePercent(5000, 1024)).toBe(100)
  })

  it('is 0 for an unlimited allowance', () => {
    expect(usagePercent(10, 0)).toBe(0)
  })
})

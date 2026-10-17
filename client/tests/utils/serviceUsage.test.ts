/**
 * Tests for `utils/serviceUsage.ts`: what counts as a limit and how the percentage is capped.
 */
import { describe, expect, it } from 'vitest'
import { hasUsageLimit, usagePercent } from '~/utils/serviceUsage'

describe('hasUsageLimit', () => {
  it.each([
    ['1024', true],
    ['0', false],
    ['', false],
    [undefined, false],
    [null, false],
  ])('reads %s as %s', (limit, expected) => {
    expect(hasUsageLimit(limit)).toBe(expected)
  })
})

describe('usagePercent', () => {
  it('divides use by allowance', () => {
    expect(usagePercent('256', '1024')).toBe(25)
  })

  it('caps the figure at 100', () => {
    expect(usagePercent('2048', '1024')).toBe(100)
  })

  it.each([
    ['abc', '1024'],
    ['10', 'abc'],
    ['10', '0'],
    [undefined, '1024'],
    ['10', undefined],
  ])('reads %s of %s as empty', (used, limit) => {
    expect(usagePercent(used, limit)).toBe(0)
  })
})

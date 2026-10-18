/**
 * Tests for `utils/domainExpiry.ts`: the 30-day window, the sentinel dates and the injected clock.
 */
import { describe, expect, it } from 'vitest'
import { EXPIRING_SOON_DAYS, EXPIRY_WARNING_DAYS, isExpiringSoon, isExpiringWithin } from '~/utils/domainExpiry'

/** A fixed "now" so no test depends on the clock. */
const NOW = Date.parse('2026-10-14T00:00:00Z')

/**
 * Builds an ISO expiry date a number of days from {@link NOW}.
 *
 * @param days - Days ahead (negative for the past).
 * @returns The ISO 8601 string.
 */
const inDays = (days: number): string => new Date(NOW + days * 86_400_000).toISOString()

describe('isExpiringSoon', () => {
  it('is true inside the window', () => {
    expect(isExpiringSoon(inDays(10), NOW)).toBe(true)
  })

  it('counts the last day of the window', () => {
    expect(isExpiringSoon(inDays(EXPIRING_SOON_DAYS), NOW)).toBe(true)
  })

  it('is false beyond the window', () => {
    expect(isExpiringSoon(inDays(EXPIRING_SOON_DAYS + 1), NOW)).toBe(false)
  })

  it('is false once the date has passed', () => {
    expect(isExpiringSoon(inDays(-1), NOW)).toBe(false)
  })

  it.each([undefined, null, '', '0001-01-01T00:00:00Z', 'not a date'])('is false for %s', (value) => {
    expect(isExpiringSoon(value, NOW)).toBe(false)
  })
})

describe('isExpiringWithin', () => {
  it('uses the given window', () => {
    expect(isExpiringWithin(inDays(EXPIRY_WARNING_DAYS), EXPIRY_WARNING_DAYS, false, NOW)).toBe(true)
    expect(isExpiringWithin(inDays(EXPIRY_WARNING_DAYS + 1), EXPIRY_WARNING_DAYS, false, NOW)).toBe(false)
  })

  it('counts a lapsed date only when asked to', () => {
    expect(isExpiringWithin(inDays(-3), EXPIRY_WARNING_DAYS, false, NOW)).toBe(false)
    expect(isExpiringWithin(inDays(-3), EXPIRY_WARNING_DAYS, true, NOW)).toBe(true)
  })

  it('never counts a sentinel date', () => {
    expect(isExpiringWithin('0001-01-01T00:00:00Z', EXPIRY_WARNING_DAYS, true, NOW)).toBe(false)
  })
})

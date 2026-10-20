/**
 * Unit tests for formatTestimonialDate: a localised month and year, UTC-stable, tolerant of
 * absent or unparseable input.
 */
import { describe, expect, it } from 'vitest'
import { formatTestimonialDate } from '~/utils/testimonialDate'

describe('formatTestimonialDate', () => {
  it('formats the month and year in English', () => {
    expect(formatTestimonialDate('2026-03-10T00:00:00+00:00', 'en')).toBe('March 2026')
  })

  it('formats in the requested locale', () => {
    expect(formatTestimonialDate('2026-03-10T00:00:00+00:00', 'ru')).toContain('март')
    expect(formatTestimonialDate('2026-03-10T00:00:00+00:00', 'ru')).toContain('2026')
  })

  it('reads the month in UTC so a month boundary does not move', () => {
    expect(formatTestimonialDate('2026-03-31T23:30:00+00:00', 'en')).toBe('March 2026')
  })

  it('returns an empty string for an absent or unparseable value', () => {
    expect(formatTestimonialDate(null, 'en')).toBe('')
    expect(formatTestimonialDate(undefined, 'en')).toBe('')
    expect(formatTestimonialDate('', 'en')).toBe('')
    expect(formatTestimonialDate('not a date', 'en')).toBe('')
  })

  it('falls back to English for an invalid locale tag', () => {
    expect(formatTestimonialDate('2026-03-10T00:00:00+00:00', 'not_a_locale!')).toBe('March 2026')
  })
})

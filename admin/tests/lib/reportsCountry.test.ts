/**
 * Tests for country naming in the reports.
 */
import { describe, expect, it } from 'vitest'
import { reportCountryName, UNKNOWN_COUNTRY } from '@/lib/reportsCountry'

describe('reportCountryName', () => {
  it('names a country by its ISO-2 code in the given language', () => {
    expect(reportCountryName('AM', 'en')).toBe('Armenia')
  })

  it('keeps the unknown marker as it is', () => {
    expect(reportCountryName(UNKNOWN_COUNTRY, 'en')).toBe(UNKNOWN_COUNTRY)
  })

  it('gives back a malformed code instead of throwing', () => {
    expect(reportCountryName('not a code', 'en')).toBe('not a code')
  })
})

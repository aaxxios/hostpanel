/**
 * Tests for `useGeoOptions`: the country list leads with the placeholder entry, phone codes resolve
 * to a "+N" string with "+1" as the fallback, and a phone string splits into country and number.
 */
import { describe, expect, it } from 'vitest'
import { useGeoOptions } from '@/composables/useGeoOptions'

describe('useGeoOptions', () => {
  it('leads the country options with the "select a country" placeholder', () => {
    const { countryOptions } = useGeoOptions()

    expect(countryOptions.value[0]?.value).toBe('')
    expect(countryOptions.value.length).toBeGreaterThan(100)
  })

  it('lists phone codes sorted by their display label', () => {
    const { phoneCodeOptions } = useGeoOptions()
    const labels = phoneCodeOptions.value.map((option) => option.displayLabel)

    expect(labels).toEqual([...labels].sort((a, b) => a.localeCompare(b)))
  })

  it('resolves a country to its phone code and falls back to +1', () => {
    const { resolvePhoneCode } = useGeoOptions()

    expect(resolvePhoneCode('AM')).toBe('+374')
    expect(resolvePhoneCode('ZZ')).toBe('+1')
  })

  it('splits a phone string into country and number', () => {
    const { parsePhone } = useGeoOptions()

    expect(parsePhone('+374 91123456')).toEqual({ countryIso2: 'AM', number: '91123456' })
  })

  it('keeps an unrecognised phone whole under the default country', () => {
    const { parsePhone } = useGeoOptions()

    expect(parsePhone('5551234567')).toEqual({ countryIso2: 'US', number: '5551234567' })
  })
})

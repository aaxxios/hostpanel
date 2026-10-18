/**
 * Unit tests for `utils/emailPreferences.ts`: decoding of the profile's email preferences.
 */
import { describe, expect, it } from 'vitest'
import { decodeEmailPrefs } from '~/utils/emailPreferences'

describe('decodeEmailPrefs', () => {
  it('treats a missing object as every category on', () => {
    const flags = decodeEmailPrefs(undefined)

    expect(Object.values(flags).every(Boolean)).toBe(true)
    expect(Object.keys(flags)).toHaveLength(6)
  })

  it('reads numbers and numeric strings alike', () => {
    const flags = decodeEmailPrefs({ general: 0, invoice: '0', support: 1, product: '1' })

    expect(flags.general).toBe(false)
    expect(flags.invoice).toBe(false)
    expect(flags.support).toBe(true)
    expect(flags.product).toBe(true)
  })

  it('treats an omitted category as on', () => {
    expect(decodeEmailPrefs({ general: 0 }).affiliate).toBe(true)
  })
})

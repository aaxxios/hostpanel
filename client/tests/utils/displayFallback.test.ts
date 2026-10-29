/**
 * Tests for the shared fallback wording: a name the backend left null or blank becomes the
 * localised "Unknown", and a real name passes through untouched.
 */

import { describe, expect, it } from 'vitest'
import { nameOrUnknown } from '~/utils/displayFallback'

/**
 * A translator that shows the key and the named values, so a test sees what was asked for.
 *
 * @param key - Message key.
 * @param named - Placeholder values.
 * @returns The key, with the values appended when there are any.
 */
const t = (key: string, named?: Record<string, unknown>): string => (named ? `${key}:${JSON.stringify(named)}` : key)

describe('nameOrUnknown', () => {
  it('passes a real name through, trimmed', () => {
    expect(nameOrUnknown(t, '  Grace ')).toBe('Grace')
  })

  it.each([null, undefined, '', '   '])('answers "Unknown" for %j', (value) => {
    expect(nameOrUnknown(t, value)).toBe('fallback.unknown')
  })
})

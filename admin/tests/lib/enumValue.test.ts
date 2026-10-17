/**
 * Unit tests for `enumValueOf`: a raw string narrows to its enum member, anything else to null.
 */
import { describe, expect, it } from 'vitest'
import { enumValueOf } from '@/lib/enumValue'

/** Enum used by the tests. */
enum Colour {
  /** Red. */
  Red = 'red',
  /** Blue. */
  Blue = 'blue',
}

describe('enumValueOf', () => {
  it('returns the member holding the value', () => {
    expect(enumValueOf(Colour, 'blue')).toBe(Colour.Blue)
  })

  it('returns null for a value no member holds', () => {
    expect(enumValueOf(Colour, 'green')).toBeNull()
    expect(enumValueOf(Colour, 'Red')).toBeNull()
  })
})

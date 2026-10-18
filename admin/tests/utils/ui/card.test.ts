/**
 * Unit tests for the card vocabulary.
 */
import { describe, expect, it } from 'vitest'
import { UiCardPadding } from '@/utils/ui/card'

describe('UiCardPadding', () => {
  it('lists unique, non-empty values', () => {
    const values = Object.values(UiCardPadding)

    expect(values.length).toBeGreaterThan(0)
    expect(new Set(values).size).toBe(values.length)
    expect(values.every((value) => value.length > 0)).toBe(true)
  })
})

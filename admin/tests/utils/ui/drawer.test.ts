/**
 * Unit tests for the drawer vocabulary.
 */
import { describe, expect, it } from 'vitest'
import { UiDrawerSide } from '@/utils/ui/drawer'

describe('UiDrawerSide', () => {
  it('lists unique, non-empty values', () => {
    const values = Object.values(UiDrawerSide)

    expect(values.length).toBeGreaterThan(0)
    expect(new Set(values).size).toBe(values.length)
    expect(values.every((value) => value.length > 0)).toBe(true)
  })
})

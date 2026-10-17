/**
 * Unit tests for the skeleton vocabulary.
 */
import { describe, expect, it } from 'vitest'
import { UiSkeletonShape } from '@/utils/ui/skeleton'

describe('UiSkeletonShape', () => {
  it('lists unique, non-empty values', () => {
    const values = Object.values(UiSkeletonShape)

    expect(values.length).toBeGreaterThan(0)
    expect(new Set(values).size).toBe(values.length)
    expect(values.every((value) => value.length > 0)).toBe(true)
  })
})

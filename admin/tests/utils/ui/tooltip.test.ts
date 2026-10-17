/**
 * Unit tests for the tooltip vocabulary.
 */
import { describe, expect, it } from 'vitest'
import { UiTooltipPlacement } from '@/utils/ui/tooltip'

describe('UiTooltipPlacement', () => {
  it('lists unique, non-empty values', () => {
    const values = Object.values(UiTooltipPlacement)

    expect(values.length).toBeGreaterThan(0)
    expect(new Set(values).size).toBe(values.length)
    expect(values.every((value) => value.length > 0)).toBe(true)
  })
})

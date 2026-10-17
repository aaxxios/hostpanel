/**
 * Unit tests for the reportField vocabulary.
 */
import { describe, expect, it } from 'vitest'
import { ReportFieldWidth } from '@/utils/ui/reportField'

describe('ReportFieldWidth', () => {
  it('lists unique, non-empty values', () => {
    const values = Object.values(ReportFieldWidth)

    expect(values.length).toBeGreaterThan(0)
    expect(new Set(values).size).toBe(values.length)
    expect(values.every((value) => value.length > 0)).toBe(true)
  })
})

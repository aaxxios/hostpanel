/**
 * Unit tests for the table vocabulary: alignment, sort direction and density classes.
 */
import { describe, expect, it } from 'vitest'
import { UiTableDensity } from '@/utils/ui/injection'
import {
  alignClasses,
  cellDensityClasses,
  headerDensityClasses,
  UiAlign,
  UiSortDirection,
} from '@/utils/ui/table'

describe('table vocabulary', () => {
  it('maps every alignment to its text- class', () => {
    for (const align of Object.values(UiAlign)) {
      expect(alignClasses[align]).toBe(`text-${align}`)
    }
  })

  it('uses the aria-sort strings for sort directions', () => {
    expect(Object.values(UiSortDirection)).toEqual(['none', 'ascending', 'descending'])
  })

  it.each([
    ['cell', cellDensityClasses],
    ['header', headerDensityClasses],
  ])('the %s map is empty for plain and padded otherwise', (_name, map) => {
    expect(map[UiTableDensity.Plain]).toBe('')
    expect(map[UiTableDensity.Compact]).toMatch(/px-/)
    expect(map[UiTableDensity.Comfortable]).toMatch(/py-/)
  })
})

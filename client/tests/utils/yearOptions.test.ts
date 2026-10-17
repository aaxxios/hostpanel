/**
 * Tests for `utils/yearOptions.ts`: the three period choices and their unit words.
 */
import { describe, expect, it } from 'vitest'
import { buildYearOptions, MAX_YEARS } from '~/utils/yearOptions'

describe('buildYearOptions', () => {
  it('offers one to three years', () => {
    expect(buildYearOptions('year', 'years').map((option) => option.value)).toEqual([1, 2, 3])
    expect(MAX_YEARS).toBe(3)
  })

  it('uses the singular word for one year and the plural for the rest', () => {
    expect(buildYearOptions('year', 'years').map((option) => option.label)).toEqual(['1 year', '2 years', '3 years'])
  })
})

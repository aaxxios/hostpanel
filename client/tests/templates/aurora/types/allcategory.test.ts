/**
 * Unit tests for the zone-list sentinel: a reserved value no registrar category can equal.
 *
 * @module tests/templates/aurora/types/allcategory.test
 */
import { describe, expect, it } from 'vitest'
import { ALL_CATEGORY } from '~/templates/aurora/types/allcategory'

describe('ALL_CATEGORY', () => {
  it('is a reserved non-empty value', () => {
    expect(ALL_CATEGORY).toBe('__all__')
    expect(ALL_CATEGORY.length).toBeGreaterThan(0)
  })
})

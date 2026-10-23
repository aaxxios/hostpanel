/**
 * Unit tests for topProductFamilies: featured first, catalogue order after, cut to the limit.
 */
import { describe, expect, it } from 'vitest'
import { HOME_PRODUCT_FAMILY_LIMIT } from '~/constants/homeProducts'
import { topProductFamilies } from '~/utils/homeProductFamilies'
import type { ProductFamily } from '~/types/productfamily'

/**
 * Builds a family with only the fields the picker reads.
 *
 * @param id Family id.
 * @param featured Whether the operator features it.
 * @returns The family.
 */
const family = (id: string, featured = false): ProductFamily => ({ id, featured } as ProductFamily)

describe('topProductFamilies', () => {
  it('keeps the catalogue order and cuts to the home limit', () => {
    const list = ['a', 'b', 'c', 'd', 'e'].map((id) => family(id))

    expect(topProductFamilies(list).map((item) => item.id)).toEqual(['a', 'b', 'c'])
    expect(HOME_PRODUCT_FAMILY_LIMIT).toBe(3)
  })

  it('puts featured families first, each group in catalogue order', () => {
    const list = [family('a'), family('b'), family('c', true), family('d', true)]

    expect(topProductFamilies(list).map((item) => item.id)).toEqual(['c', 'd', 'a'])
  })

  it('returns fewer when there are fewer, and does not change its input', () => {
    const list = [family('a'), family('b', true)]

    expect(topProductFamilies(list).map((item) => item.id)).toEqual(['b', 'a'])
    expect(list.map((item) => item.id)).toEqual(['a', 'b'])
  })

  it('honours an explicit limit', () => {
    expect(topProductFamilies([family('a'), family('b')], 1)).toHaveLength(1)
    expect(topProductFamilies([family('a')], 0)).toEqual([])
  })
})

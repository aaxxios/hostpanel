/**
 * Tests for the page arithmetic: page count, in-memory slice and the first and last row numbers.
 */
import { describe, expect, it } from 'vitest'
import { pageCount, pageFirstRow, pageLastRow, pageSlice } from '@/lib/paging'

describe('pageCount', () => {
  it('rounds up to whole pages', () => {
    expect(pageCount(41, 20)).toBe(3)
    expect(pageCount(40, 20)).toBe(2)
  })

  it('keeps one page for an empty list', () => {
    expect(pageCount(0, 20)).toBe(1)
  })
})

describe('pageSlice', () => {
  const items = [1, 2, 3, 4, 5]

  it('cuts the rows of the page', () => {
    expect(pageSlice(items, 1, 2)).toEqual([1, 2])
    expect(pageSlice(items, 2, 2)).toEqual([3, 4])
  })

  it('returns the short last page and nothing past it', () => {
    expect(pageSlice(items, 3, 2)).toEqual([5])
    expect(pageSlice(items, 4, 2)).toEqual([])
  })
})

describe('page row numbers', () => {
  it('numbers the first and last row of a page', () => {
    expect(pageFirstRow(1, 20)).toBe(1)
    expect(pageFirstRow(3, 20)).toBe(41)
    expect(pageLastRow(1, 20, 55)).toBe(20)
  })

  it('stops the last row at the total', () => {
    expect(pageLastRow(3, 20, 55)).toBe(55)
  })
})

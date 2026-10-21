/**
 * Unit tests for the balanced-grid column helpers: the column count per item count, the tablet
 * cap and the size of the last row.
 */
import { describe, expect, it } from 'vitest'
import { BALANCED_MAX_COLUMNS, balancedColumns, lastRowSize, tabletColumns } from '~/utils/balancedGrid'

describe('balancedColumns', () => {
  it.each([
    [0, 1], [1, 1], [2, 2], [3, 3], [4, 4],
    [5, 3], [6, 3], [7, 4], [8, 4], [9, 3], [10, 4], [11, 4], [12, 3],
  ])('uses the right columns for %i items', (count, expected) => {
    expect(balancedColumns(count)).toBe(expected)
  })

  it('never exceeds the maximum, whatever the count', () => {
    for (let count = 0; count <= 60; count++) {
      expect(balancedColumns(count)).toBeLessThanOrEqual(BALANCED_MAX_COLUMNS)
    }
  })

  it('never leaves a single orphan when a better width exists', () => {
    for (let count = 5; count <= 60; count++) {
      const columns = balancedColumns(count)
      const otherFill = [3, 4].map(width => lastRowSize(count, width))

      // A lone card on the last row is only acceptable when no width avoids it.
      if (lastRowSize(count, columns) === 1) {
        expect(Math.max(...otherFill)).toBe(1)
      }
    }
  })
})

describe('tabletColumns', () => {
  it('caps at two and never exceeds the item count', () => {
    expect(tabletColumns(1)).toBe(1)
    expect(tabletColumns(2)).toBe(2)
    expect(tabletColumns(9)).toBe(2)
    expect(tabletColumns(0)).toBe(1)
  })
})

describe('lastRowSize', () => {
  it('is a full row when the count divides evenly, and zero for no items', () => {
    expect(lastRowSize(6, 3)).toBe(3)
    expect(lastRowSize(7, 4)).toBe(3)
    expect(lastRowSize(5, 3)).toBe(2)
    expect(lastRowSize(0, 3)).toBe(0)
  })
})

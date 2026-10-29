import { describe, expect, it } from 'vitest'
import { buildComparisonRows, commonRowCount, isComparable } from '~/utils/comparisonRows'
import type { ProductFeature } from '~/types/productfeature'

/**
 * Builds a specification line.
 *
 * @param productId Product the line belongs to.
 * @param label Feature name.
 * @param value Value for this product.
 * @param sortOrder Position within the product.
 * @returns The line.
 */
const line = (productId: number, label: string, value: string, sortOrder: number): ProductFeature =>
  ({ id: productId * 100 + sortOrder, productId, label, value, sortOrder })

describe('buildComparisonRows', () => {
  it('has nothing to compare without plans', () => {
    expect(buildComparisonRows([line(1, 'Disk', '10 GB', 0)], [])).toEqual([])
  })

  it('has nothing to compare when no plan specifies anything', () => {
    expect(buildComparisonRows([], [1, 2])).toEqual([])
  })

  it('aligns each plan value to its column', () => {
    const features = [
      line(1, 'Disk', '10 GB', 0),
      line(1, 'Bandwidth', '1 TB', 1),
      line(2, 'Disk', '50 GB', 0),
      line(2, 'Bandwidth', '10 TB', 1),
    ]

    expect(buildComparisonRows(features, [1, 2])).toEqual([
      { label: 'Disk', values: ['10 GB', '50 GB'] },
      { label: 'Bandwidth', values: ['1 TB', '10 TB'] },
    ])
  })

  it('follows the column order it is given, not the order of the input', () => {
    const features = [line(2, 'Disk', '50 GB', 0), line(1, 'Disk', '10 GB', 0)]

    expect(buildComparisonRows(features, [2, 1])).toEqual([
      { label: 'Disk', values: ['50 GB', '10 GB'] },
    ])
  })

  it('leaves a gap rather than shifting a column when a plan omits a line', () => {
    const features = [
      line(1, 'Disk', '10 GB', 0),
      line(2, 'Disk', '50 GB', 0),
      line(2, 'Backups', 'Daily', 1),
    ]

    expect(buildComparisonRows(features, [1, 2])).toEqual([
      { label: 'Disk', values: ['10 GB', '50 GB'] },
      { label: 'Backups', values: ['', 'Daily'] },
    ])
  })

  it('keeps a row a single plan uses, so nothing an operator entered is dropped', () => {
    const features = [line(2, 'Dedicated IP', 'Included', 0)]

    expect(buildComparisonRows(features, [1, 2])).toEqual([
      { label: 'Dedicated IP', values: ['', 'Included'] },
    ])
  })

  it('orders rows by sort order within a plan, regardless of input order', () => {
    const features = [
      line(1, 'Bandwidth', '1 TB', 2),
      line(1, 'Disk', '10 GB', 1),
      line(1, 'Databases', '5', 3),
    ]

    expect(buildComparisonRows(features, [1]).map(r => r.label))
      .toEqual(['Disk', 'Bandwidth', 'Databases'])
  })
})

describe('isComparable', () => {
  it('counts only rows at least two plans have a value for', () => {
    const rows = [{ label: 'A', values: ['1', '2'] }, { label: 'B', values: ['1', ''] }, { label: 'C', values: ['', ' '] }]

    expect(commonRowCount(rows)).toBe(1)
  })

  it('needs two shared rows, so plans with no common features get no table', () => {
    expect(isComparable([])).toBe(false)
    expect(isComparable([{ label: 'A', values: ['1', '2'] }])).toBe(false)
    expect(isComparable([{ label: 'A', values: ['1', ''] }, { label: 'B', values: ['', '2'] }])).toBe(false)
    expect(isComparable([{ label: 'A', values: ['1', '2'] }, { label: 'B', values: ['3', '4'] }])).toBe(true)
  })
})

describe('prod-shaped plans', () => {
  /** The real operator plans, left to right. */
  const planIds = [11, 12, 13, 14, 15, 16]

  it('has no rows and no table when none of the six plans has a specification line', () => {
    const rows = buildComparisonRows([], planIds)

    expect(rows).toEqual([])
    expect(isComparable(rows)).toBe(false)
  })

  it('has no table when every plan labels its lines differently', () => {
    const features = planIds.map((id, index) => line(id, `Label ${index}`, `${index} GB`, 0))
    const rows = buildComparisonRows(features, planIds)

    // Every label is kept as a row, but no row is shared by two plans.
    expect(rows).toHaveLength(6)
    expect(commonRowCount(rows)).toBe(0)
    expect(isComparable(rows)).toBe(false)
  })
})

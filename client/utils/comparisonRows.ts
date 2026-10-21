/**
 * Aligns per-product specification lines into comparison rows.
 *
 * @module utils/comparisonRows
 */

import type { ComparisonRow } from '~/types/comparisonrow'
import type { ProductFeature } from '~/types/productfeature'

/**
 * Turns per-product specification lines into aligned comparison rows.
 *
 * Rows are keyed by label, which is what makes the table line up: products are
 * walked in display order and each new label is appended the first time it is
 * seen, so the leftmost plan sets the running order and later plans contribute
 * only the lines it does not already have. A plan missing a line gets an empty
 * value rather than shifting the column, and a label only one plan uses still
 * gets its own row.
 *
 * @param features Specification lines for any subset of the products.
 * @param productIds Products to compare, in the order their columns appear.
 * @returns Rows in display order; empty when nothing is specified.
 */
export const buildComparisonRows = (
  features: ProductFeature[],
  productIds: number[],
): ComparisonRow[] => {
  if (productIds.length === 0) {
    return []
  }

  const byProduct = new Map<number, ProductFeature[]>()
  for (const feature of features) {
    const list = byProduct.get(feature.productId)
    if (list) {
      list.push(feature)
    } else {
      byProduct.set(feature.productId, [feature])
    }
  }

  const order: string[] = []
  const seen = new Set<string>()
  for (const id of productIds) {
    /** This product's lines in the order the operator set. */
    const lines = (byProduct.get(id) ?? []).slice().sort((a, b) => a.sortOrder - b.sortOrder)
    for (const line of lines) {
      if (!seen.has(line.label)) {
        seen.add(line.label)
        order.push(line.label)
      }
    }
  }

  return order.map(label => ({
    label,
    values: productIds.map(id =>
      byProduct.get(id)?.find(line => line.label === label)?.value ?? ''),
  }))
}

/** Fewest shared rows a comparison table needs before it adds value. */
export const MIN_COMMON_ROWS = 2

/** Fewest plans that must carry a line for it to count as shared. */
export const MIN_SHARED_PLANS = 2

/**
 * How many rows at least two plans have a value for.
 *
 * @param rows Aligned comparison rows.
 * @returns The count of shared rows.
 */
export const commonRowCount = (rows: readonly ComparisonRow[]): number =>
  rows.filter(row => row.values.filter(value => value.trim() !== '').length >= MIN_SHARED_PLANS).length

/**
 * Whether the plans share enough lines for a comparison table to be worth showing.
 *
 * Plans whose specifications barely overlap would render a wall of "not included"; the table is
 * hidden instead, and the plan cards carry the information.
 *
 * @param rows Aligned comparison rows.
 * @returns True when at least {@link MIN_COMMON_ROWS} rows are shared.
 */
export const isComparable = (rows: readonly ComparisonRow[]): boolean => commonRowCount(rows) >= MIN_COMMON_ROWS

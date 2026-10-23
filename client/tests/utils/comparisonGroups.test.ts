/**
 * Unit tests for buildComparisonGroups: only comparable, non-domain groups with two or more plans
 * become tables, in display order, with the payer-currency price and the featured plan flagged.
 */
import { describe, expect, it } from 'vitest'
import { buildComparisonGroups } from '~/utils/comparisonGroups'
import { PricingCycle } from '~/types/pricingcycle'
import type { PortalProduct } from '~/types/portalproduct'
import type { PortalProductGroup } from '~/types/portalproductgroup'
import type { ProductFeature } from '~/types/productfeature'

/**
 * Builds a catalogue group.
 *
 * @param id The group id.
 * @returns The group.
 */
const group = (id: number): PortalProductGroup => ({ id, name: `Group ${id}`, isActive: true, productCount: 2 })

/**
 * Builds a plan.
 *
 * @param id The product id.
 * @param groupId The group it belongs to.
 * @param monthly Its monthly price.
 * @param isFeatured Whether the operator recommends it.
 * @returns The plan.
 */
const plan = (id: number, groupId: number, monthly: number, isFeatured = false): PortalProduct =>
  ({ id, groupId, name: `Plan ${id}`, isFeatured, pricing: { monthly, annual: null } }) as unknown as PortalProduct

/**
 * Builds a specification line.
 *
 * @param productId The product.
 * @param label The line name.
 * @returns The line.
 */
const line = (productId: number, label: string): ProductFeature => ({ id: productId * 10 + label.length, productId, label, value: '1', sortOrder: 0 })

/** Two shared lines for products 1 and 2. */
const SHARED = [line(1, 'Disk'), line(2, 'Disk'), line(1, 'Mail'), line(2, 'Mail')]

/**
 * Formats an amount.
 *
 * @param amount The amount.
 * @returns The text.
 */
const format = (amount: number): string => `$${amount}`

describe('buildComparisonGroups', () => {
  it('builds columns and rows from the real plans', () => {
    const [result] = buildComparisonGroups([group(1)], [plan(2, 1, 9), plan(1, 1, 5)], format, () => SHARED)

    expect(result?.plans.map(p => p.name)).toEqual(['Plan 1', 'Plan 2'])
    expect(result?.plans.map(p => p.price)).toEqual(['$5', '$9'])
    expect(result?.plans[0]?.billingCycle).toBe(PricingCycle.Monthly)
    expect(result?.rows.map(r => r.label)).toEqual(['Disk', 'Mail'])
  })

  it('flags only the first featured paid plan', () => {
    const [result] = buildComparisonGroups([group(1)], [plan(1, 1, 5), plan(2, 1, 9, true)], format, () => SHARED)

    expect(result?.plans.map(p => p.isFeatured)).toEqual([false, true])
  })

  it('leaves out a group whose plans are not comparable', () => {
    expect(buildComparisonGroups([group(1)], [plan(1, 1, 5), plan(2, 1, 9)], format, () => [line(1, 'Disk')])).toEqual([])
  })

  it('leaves out a group with a single plan', () => {
    expect(buildComparisonGroups([group(1)], [plan(1, 1, 5)], format, () => SHARED)).toEqual([])
  })

  it('keeps only the groups that qualify', () => {
    const result = buildComparisonGroups([group(1), group(2)], [plan(1, 1, 5), plan(2, 1, 9), plan(3, 2, 5), plan(4, 2, 9)], format, id => (id === 1 ? SHARED : []))

    expect(result.map(g => g.id)).toEqual([1])
  })
})

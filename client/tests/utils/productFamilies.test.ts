/**
 * Unit tests for the product-family helpers: the first offered price, the pricing rows, the grouping
 * of the catalogue into families and the lookup of one product for its detail page.
 */
import { describe, expect, it } from 'vitest'
import { PricingCycle } from '~/types/pricingcycle'
import type { PortalProduct } from '~/types/portalproduct'
import type { ProductFamily } from '~/types/productfamily'
import { buildPricingRows, buildProductFamilies, cycleAmount, findProductDetail, firstOfferedPrice } from '~/utils/productFamilies'

/**
 * Builds a catalogue product.
 *
 * @param overrides Fields that differ from the defaults.
 * @returns The product.
 */
const product = (overrides: Partial<PortalProduct> & Pick<PortalProduct, 'id' | 'groupId' | 'name'>): PortalProduct => ({
  pid: overrides.id,
  description: '',
  slug: null,
  pricing: null,
  ...overrides,
})

/** Formats an amount the way the tests expect to read it. */
const dollars = (amount: number): string => `$${amount}`

/** A family group: the parent that names it and two plans. */
const learnGroup: PortalProduct[] = [
  product({ id: 10, groupId: 3, name: 'SmartLearn System', slug: 'smartlearn-system', description: 'Learn online<br />✔ Courses<br />✔ Exams' }),
  product({ id: 11, groupId: 3, name: 'Starter', slug: 'starter', description: '✔ One site', pricing: { monthly: 10, annual: 100 } }),
  product({ id: 12, groupId: 3, name: 'Pro', slug: 'pro', description: '✔ Two sites', pricing: { monthly: null, annual: 200 } }),
]

describe('firstOfferedPrice', () => {
  it('prefers the monthly price', () => {
    expect(firstOfferedPrice({ monthly: 5, annual: 50 })).toEqual({ cycle: PricingCycle.Monthly, amount: 5 })
  })

  it('falls back to the annual price when there is no monthly one', () => {
    expect(firstOfferedPrice({ monthly: null, annual: 50 })).toEqual({ cycle: PricingCycle.Annual, amount: 50 })
  })

  it('keeps a free (zero) monthly price', () => {
    expect(firstOfferedPrice({ monthly: 0, annual: 50 })).toEqual({ cycle: PricingCycle.Monthly, amount: 0 })
  })

  it('answers null when the plan carries no price', () => {
    expect(firstOfferedPrice({ monthly: null, annual: null })).toBeNull()
    expect(firstOfferedPrice(null)).toBeNull()
    expect(firstOfferedPrice(undefined)).toBeNull()
  })
})

describe('buildPricingRows', () => {
  it('maps each plan to a row with its first price and parsed features', () => {
    const rows = buildPricingRows(learnGroup.slice(1), dollars, 'USD')

    expect(rows).toHaveLength(2)
    expect(rows[0]).toMatchObject({ pid: 11, tier: 'Starter', price: '$10', amount: 10, currency: 'USD', billingCycle: PricingCycle.Monthly, features: ['One site'] })
    expect(rows[1]).toMatchObject({ pid: 12, price: '$200', billingCycle: PricingCycle.Annual })
  })

  it('leaves the price empty for a plan without one, and the currency empty while it resolves', () => {
    const rows = buildPricingRows([product({ id: 1, groupId: 3, name: 'Free-ish' })], dollars, null)

    expect(rows[0]).toMatchObject({ price: '', amount: 0, currency: '', billingCycle: PricingCycle.Monthly })
  })
})

describe('buildProductFamilies', () => {
  it('builds one family per group, priced by its sub-plans and named by its parent', () => {
    const families = buildProductFamilies(learnGroup, dollars, 'USD')

    expect(families).toHaveLength(1)
    expect(families[0]).toMatchObject({ id: 'smartlearn-system', gid: 3, tagline: '', description: 'Learn online' })
    expect(families[0]?.pricing.map((row) => row.pid)).toEqual([11, 12])
    expect(families[0]?.features).toEqual(expect.arrayContaining(['Courses', 'Exams', 'One site', 'Two sites']))
  })

  it('prices a group that has only its parent by the parent alone', () => {
    const families = buildProductFamilies(
      [product({ id: 20, groupId: 4, name: 'PropSystem Pro', slug: 'propsystem-pro', pricing: { monthly: 7, annual: null } })],
      dollars,
      'USD',
    )

    expect(families[0]?.pricing).toHaveLength(1)
    expect(families[0]?.pricing[0]).toMatchObject({ pid: 20, price: '$7' })
  })

  it('leaves out a group that has no products, and puts the leading entries first', () => {
    const leading: ProductFamily = {
      id: 'domains', gid: null, icon: 'earth', color: '#3b82f6', learnMoreUrl: '/domains', name: 'Domains',
      tagline: 'Tag', description: 'Desc', features: [], pricing: [],
    }

    const families = buildProductFamilies(learnGroup, dollars, 'USD', [leading])

    expect(families.map((family) => family.id)).toEqual(['domains', 'smartlearn-system'])
  })

  it('caps the aggregated feature list at six lines', () => {
    const many = product({
      id: 30, groupId: 5, name: 'ShopKit Pro', slug: 'shopkit-pro',
      description: ['One', 'Two', 'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight'].map((word) => `✔ ${word}`).join('<br />'),
    })

    expect(buildProductFamilies([many], dollars, 'USD')[0]?.features).toHaveLength(6)
  })
})

describe('findProductDetail', () => {
  it('finds a product by its slug and shapes its plans', () => {
    const detail = findProductDetail(learnGroup, 'smartlearn-system')

    expect(detail).toMatchObject({ id: 'smartlearn-system', name: 'SmartLearn System', color: '#8b5cf6', description: 'Learn online' })
    expect(detail?.plans.map((plan) => plan.tier)).toEqual(['Starter', 'Pro'])
    expect(detail?.plans[0]?.pricing).toEqual({ monthly: 10, annual: 100 })
  })

  it('falls back to the first product of the family group when the slug differs', () => {
    const detail = findProductDetail([product({ id: 40, groupId: 3, name: 'Odd name', slug: 'odd' })], 'smartlearn-system')

    expect(detail?.id).toBe('smartlearn-system')
    expect(detail?.name).toBe('Odd name')
  })

  it('answers null when nothing matches', () => {
    expect(findProductDetail(learnGroup, 'no-such-product')).toBeNull()
    expect(findProductDetail([], 'smartlearn-system')).toBeNull()
  })

  it('ignores products that are not in a family group', () => {
    expect(findProductDetail([product({ id: 50, groupId: 99, name: 'Stray', slug: 'stray' })], 'stray')).toBeNull()
  })
})

describe('cycleAmount', () => {
  it('reads the price of the cycle', () => {
    expect(cycleAmount({ monthly: 5, annual: 50 }, PricingCycle.Annual)).toBe(50)
  })

  it('is null when the plan has no price for the cycle or no prices at all', () => {
    expect(cycleAmount({ monthly: 5, annual: null }, PricingCycle.Annual)).toBeNull()
    expect(cycleAmount(null, PricingCycle.Monthly)).toBeNull()
    expect(cycleAmount(undefined, PricingCycle.Monthly)).toBeNull()
  })
})

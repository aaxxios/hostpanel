/**
 * Unit tests for ProductsComparison: only families with priced plans get a row, the first three
 * plans fill the Starter, Professional and Enterprise columns, and each family links to its page.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import ProductsComparison from '~/components/catalog/ProductsComparison.vue'
import MarketingSectionHeader from '~/components/marketing/MarketingSectionHeader.vue'
import { PricingCycle } from '~/types/pricingcycle'
import type { ProductFamily } from '~/types/productfamily'
import type { ProductPricingRow } from '~/types/productpricingrow'

/**
 * Builds a pricing row.
 *
 * @param price The formatted price.
 * @returns The row.
 */
const row = (price: string): ProductPricingRow => ({
  pid: 1, tier: 'Tier', slug: '', price, amount: 1, currency: 'USD', billingCycle: PricingCycle.Monthly, features: [],
})

/**
 * Builds a family.
 *
 * @param id The family id.
 * @param pricing Its pricing rows.
 * @returns The family.
 */
const family = (id: string, pricing: ProductPricingRow[]): ProductFamily => ({
  id, gid: 3, icon: 'cube', color: '#06b6d4', learnMoreUrl: `/products/${id}`, name: `Name ${id}`,
  tagline: '', description: '', features: [], pricing,
})

/**
 * Mounts the table.
 *
 * @param products The families to show.
 * @returns The wrapper.
 */
const mountTable = (products: ProductFamily[]) =>
  mountWithKit(ProductsComparison, {
    props: { products },
    components: { MarketingSectionHeader },
    stubs: { 'i18n-t': true },
  })

describe('ProductsComparison', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
  })

  it('gives a row only to the families that have priced plans', () => {
    const wrapper = mountTable([family('a', [row('$1')]), family('b', [])])

    expect(wrapper.find('[data-testid="comparison-row-a"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="comparison-row-b"]').exists()).toBe(false)
  })

  it('puts the first three plan prices in the three price columns', () => {
    const wrapper = mountTable([family('a', [row('$1'), row('$2'), row('$3')])])

    const cells = wrapper.findAll('[data-testid="comparison-row-a"] td').map((cell) => cell.text())
    expect(cells[1]).toBe('$1')
    expect(cells[2]).toBe('$2')
    expect(cells[3]).toBe('$3')
  })

  it('links the family name to its page and tints the middle price with its accent', () => {
    const wrapper = mountTable([family('a', [row('$1'), row('$2')])])

    expect(wrapper.find('[data-testid="comparison-row-a"] a').attributes('href')).toBe('/en/products/a')
    const accents = wrapper.findAll('[data-testid="comparison-row-a"] [data-testid="ui-accent"]')
    expect((accents[1]?.element as HTMLElement).style.getPropertyValue('--accent')).toBe('#06b6d4')
  })

  it('renders the column headings and the footer note', () => {
    const wrapper = mountTable([family('a', [row('$1')])])

    expect(wrapper.text()).toContain('products.comparison.headers.starter')
    expect(wrapper.text()).toContain('products.comparison.headers.professionalBadge')
    expect(wrapper.text()).toContain('products.comparison.footer')
  })
})

/**
 * Unit tests for ProductDetail: the header block (family name, description, key bullets), the
 * plans block (shared segmented cycle control, balanced grid, plan cards), the comparison block
 * that appears only for comparable plans, the FAQ slot and the closing "not sure" band.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import ProductDetail from '~/components/catalog/ProductDetail.vue'
import CatalogProductPlanCard from '~/components/catalog/ProductPlanCard.vue'
import CatalogProductPlansTable from '~/components/catalog/ProductPlansTable.vue'
import CatalogProductSectionHeader from '~/components/catalog/ProductSectionHeader.vue'
import type { ComparisonRow } from '~/types/comparisonrow'
import { PricingCycle } from '~/types/pricingcycle'
import type { ProductDetail as ProductDetailData } from '~/types/productdetail'
import type { ProductPlan } from '~/types/productplan'
import { balancedColumns } from '~/utils/balancedGrid'

/**
 * Builds a plan.
 *
 * @param id Product id.
 * @param overrides Fields that differ from the defaults.
 * @returns The plan.
 */
const plan = (id: number, overrides: Partial<ProductPlan> = {}): ProductPlan => ({
  id,
  tier: `Plan ${id}`,
  pricing: { monthly: id, annual: id * 10 },
  description: '',
  features: [],
  isFeatured: false,
  ...overrides,
})

/** The family the tests render. */
const product: ProductDetailData = {
  id: 'alpha-suite',
  gid: 3,
  name: 'Alpha Suite',
  icon: 'school',
  color: '#8b5cf6',
  learnMoreUrl: '/products/alpha-suite',
  description: 'Learn online',
  features: ['Courses', 'Exams'],
  plans: [
    plan(11, { tier: 'Starter', features: ['One site'] }),
    plan(12, { tier: 'Pro', features: ['Two sites'] }),
    plan(13, { tier: 'Enterprise', pricing: { monthly: 30, annual: null } }),
  ],
}

/** Rows two plans share, so the family counts as comparable. */
const comparableRows: ComparisonRow[] = [
  { label: 'Disk', values: ['10 GB', '20 GB', ''] },
  { label: 'Sites', values: ['1', '2', ''] },
]

/**
 * Mounts the detail with sensible defaults.
 *
 * @param props Props that differ from the defaults.
 * @param slots Slot content by slot name.
 * @returns The wrapper.
 */
const mountDetail = (props: Record<string, unknown> = {}, slots: Record<string, string> = {}) =>
  mountWithKit(ProductDetail, {
    props: {
      product,
      cycles: [PricingCycle.Monthly, PricingCycle.Annual],
      selectedCycle: PricingCycle.Monthly,
      priceFor: (item: ProductPlan) => `$${item.pricing?.monthly ?? ''}`,
      isInCart: () => false,
      trialUrlFor: () => 'https://orders.test/cart',
      ...props,
    },
    slots,
    components: { CatalogProductPlanCard, CatalogProductPlansTable, CatalogProductSectionHeader },
  })

describe('ProductDetail', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
  })

  it('shows the family name as the h1, its description and its key bullets in the header block', () => {
    const header = mountDetail().find('[data-testid="product-header"]')

    expect(header.find('[data-testid="product-name"]').element.tagName).toBe('H1')
    expect(header.find('[data-testid="product-name"]').text()).toBe('Alpha Suite')
    expect(header.find('[data-testid="product-description"]').text()).toBe('Learn online')
    expect(header.find('[data-testid="product-features"]').text()).toContain('Courses')
    expect(header.find('[data-testid="product-features"]').text()).toContain('Exams')
  })

  it('leaves out the description and the bullets the family does not have', () => {
    const wrapper = mountDetail({ product: { ...product, description: '', features: [] } })

    expect(wrapper.find('[data-testid="product-description"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="product-features"]').exists()).toBe(false)
  })

  it('paints the section in the product accent colour', () => {
    const accent = mountDetail().find('[data-testid="ui-accent"]').element as HTMLElement

    expect(accent.style.getPropertyValue('--accent')).toBe('#8b5cf6')
  })

  it('opens every block with the same section heading and keeps them in one container width', () => {
    const wrapper = mountDetail({ comparisonRows: comparableRows })

    expect(wrapper.findAll('[data-testid="product-section-header"]')).toHaveLength(2)
    for (const block of ['product-header', 'product-plans', 'product-compare', 'product-help']) {
      expect(wrapper.find(`[data-testid="${block}"] .container-custom`).exists()).toBe(true)
    }
  })

  it('renders a card per plan inside a balanced grid and marks the middle one as popular', () => {
    const wrapper = mountDetail()
    const grid = wrapper.find('[data-testid="product-plans-grid"]')

    expect(grid.attributes('data-columns')).toBe(String(balancedColumns(3)))
    expect(grid.findAll('[data-testid^="product-plan-1"]').length).toBeGreaterThanOrEqual(3)
    expect(wrapper.find('[data-testid="product-plan-11"] [data-testid="product-plan-price"]').text()).toBe('$11')
    expect(wrapper.find('[data-testid="product-plan-12"]').text()).toContain('products.pricing.popular')
    expect(wrapper.find('[data-testid="product-plan-11"]').text()).not.toContain('products.pricing.popular')
  })

  it('centres five plans in a balanced grid instead of leaving an orphan row', () => {
    const five = [11, 12, 13, 14, 15].map((id) => plan(id))
    const grid = mountDetail({ product: { ...product, plans: five } }).find('[data-testid="product-plans-grid"]')

    expect(grid.attributes('data-columns')).toBe(String(balancedColumns(5)))
    expect(grid.classes()).toContain('justify-center')
  })

  it('shows a dash for a plan without a price in the selected cycle', () => {
    const wrapper = mountDetail({ priceFor: () => '' })

    expect(wrapper.find('[data-testid="product-plan-11"] [data-testid="product-plan-price"]').text()).toBe('—')
  })

  it('offers the cycle choice through the shared segmented control only when there is more than one cycle, and reports the pick', async () => {
    const both = mountDetail()
    expect(both.find('[data-testid="product-cycles"]').attributes('role')).toBe('radiogroup')
    expect(both.find('[data-testid="product-cycle-monthly"]').attributes('aria-checked')).toBe('true')

    await both.find('[data-testid="product-cycle-annual"]').trigger('click')
    expect(both.emitted('update:selectedCycle')?.[0]).toEqual([PricingCycle.Annual])

    const single = mountDetail({ cycles: [PricingCycle.Monthly] })
    expect(single.find('[data-testid="product-cycles"]').exists()).toBe(false)
  })

  it('reports the plan the visitor adds to the cart, and shows "in cart" for one already there', async () => {
    const wrapper = mountDetail({ isInCart: (item: ProductPlan) => item.id === 12 })

    await wrapper.find('[data-testid="product-add-11"]').trigger('click')

    expect(wrapper.emitted('addToCart')?.[0]).toEqual([product.plans[0]])
    expect(wrapper.find('[data-testid="product-add-12"]').text()).toContain('hosting.inCart')
    expect(wrapper.find('[data-testid="product-add-11"]').text()).toContain('hosting.addToCart')
  })

  it('shows the trial button only for a plan that has an external order page', async () => {
    const wrapper = mountDetail({ trialUrlFor: (item: ProductPlan) => (item.id === 11 ? 'https://orders.test/cart' : null) })

    expect(wrapper.find('[data-testid="product-trial-12"]').exists()).toBe(false)
    await wrapper.find('[data-testid="product-trial-11"]').trigger('click')
    expect(wrapper.emitted('trial')?.[0]).toEqual([product.plans[0]])
  })

  it('draws the comparison block only when the plans are comparable', () => {
    expect(mountDetail().find('[data-testid="product-compare"]').exists()).toBe(false)

    const wrapper = mountDetail({ comparisonRows: comparableRows })
    expect(wrapper.find('[data-testid="product-compare"]').attributes('id')).toBe('compare-plans')
    expect(wrapper.find('[data-testid="product-compare-row-Disk"]').text()).toContain('20 GB')
  })

  it('does not draw a comparison for rows that barely overlap', () => {
    const sparse: ComparisonRow[] = [{ label: 'Disk', values: ['10 GB', '', ''] }]

    expect(mountDetail({ comparisonRows: sparse }).find('[data-testid="product-compare"]').exists()).toBe(false)
  })

  it('renders the FAQ slot between the comparison and the closing band', () => {
    const html = mountDetail({ comparisonRows: comparableRows }, { faq: '<div data-testid="faq-slot">faq</div>' }).html()

    expect(html.indexOf('product-compare"')).toBeLessThan(html.indexOf('faq-slot'))
    expect(html.indexOf('faq-slot')).toBeLessThan(html.indexOf('product-help'))
  })

  it('closes with the "not sure" band: compare plans and contact support, no demo', () => {
    const band = mountDetail().find('[data-testid="product-help"]')

    expect(band.text()).toContain('products.help.title')
    expect(band.find('[data-testid="product-contact"]').attributes('href')).toBe('/en/contact')
    expect(band.find('[data-testid="product-contact"]').text()).toContain('products.help.contact')
    expect(band.find('[data-testid="product-compare-link"]').text()).toContain('products.help.compare')
    expect(band.find('[data-testid="product-demo"]').exists()).toBe(false)
  })

  it('points "Compare plans" at the comparison on this page when there is one', () => {
    const wrapper = mountDetail({ comparisonRows: comparableRows })

    expect(wrapper.find('[data-testid="product-compare-link"]').attributes('href')).toBe('#compare-plans')
  })

  it('points "Compare plans" at the hosting comparison for the hosting family and at the products page otherwise', () => {
    const hosting = mountDetail({ product: { ...product, learnMoreUrl: '/hosting' } })
    expect(hosting.find('[data-testid="product-compare-link"]').attributes('href')).toBe('/en/hosting#compare-plans')

    expect(mountDetail().find('[data-testid="product-compare-link"]').attributes('href')).toBe('/en/products')
  })
})

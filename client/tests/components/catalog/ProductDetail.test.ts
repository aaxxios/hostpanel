/**
 * Unit tests for ProductDetail: the header, the key features, the cycle choice, the plan panels
 * (price, cart button, trial button) and the demo and contact calls to action.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import ProductDetail from '~/components/catalog/ProductDetail.vue'
import { PricingCycle } from '~/types/pricingcycle'
import type { ProductDetail as ProductDetailData } from '~/types/productdetail'
import type { ProductPlan } from '~/types/productplan'

/** The product the tests render. */
const product: ProductDetailData = {
  id: 'smartlearn-system',
  pid: 10,
  name: 'SmartLearn System',
  icon: 'school',
  color: '#8b5cf6',
  learnMoreUrl: '/products/smartlearn-system',
  description: 'Learn online',
  features: ['Courses', 'Exams'],
  plans: [
    { pid: 11, tier: 'Starter', pricing: { monthly: 10, annual: 100 }, features: ['One site'] },
    { pid: 12, tier: 'Pro', pricing: { monthly: 20, annual: 200 }, features: ['Two sites'] },
    { pid: 13, tier: 'Enterprise', pricing: { monthly: 30, annual: null }, features: [] },
  ],
}

/**
 * Mounts the detail with sensible defaults.
 *
 * @param props Props that differ from the defaults.
 * @returns The wrapper.
 */
const mountDetail = (props: Record<string, unknown> = {}) =>
  mountWithKit(ProductDetail, {
    props: {
      product,
      cycles: [PricingCycle.Monthly, PricingCycle.Annual],
      selectedCycle: PricingCycle.Monthly,
      priceFor: (plan: ProductPlan) => `$${plan.pricing?.monthly ?? ''}`,
      isInCart: () => false,
      trialUrlFor: () => 'https://orders.test/cart',
      ...props,
    },
  })

describe('ProductDetail', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
  })

  it('shows the product name, description and key features', () => {
    const wrapper = mountDetail()

    expect(wrapper.find('[data-testid="product-name"]').element.tagName).toBe('H1')
    expect(wrapper.find('[data-testid="product-name"]').text()).toBe('SmartLearn System')
    expect(wrapper.find('[data-testid="product-description"]').text()).toBe('Learn online')
    expect(wrapper.text()).toContain('Courses')
    expect(wrapper.text()).toContain('Exams')
  })

  it('paints the section in the product accent colour', () => {
    const accent = mountDetail().find('[data-testid="ui-accent"]').element as HTMLElement

    expect(accent.style.getPropertyValue('--accent')).toBe('#8b5cf6')
  })

  it('renders a panel per plan with its price and marks the middle one as popular', () => {
    const wrapper = mountDetail()

    expect(wrapper.findAll('[data-testid^="product-plan-1"]').length).toBeGreaterThanOrEqual(3)
    expect(wrapper.find('[data-testid="product-plan-11"] [data-testid="product-plan-price"]').text()).toBe('$10')
    expect(wrapper.find('[data-testid="product-plan-12"]').text()).toContain('products.pricing.popular')
    expect(wrapper.find('[data-testid="product-plan-11"]').text()).not.toContain('products.pricing.popular')
  })

  it('shows a dash for a plan without a price in the selected cycle', () => {
    const wrapper = mountDetail({ priceFor: () => '' })

    expect(wrapper.find('[data-testid="product-plan-11"] [data-testid="product-plan-price"]').text()).toBe('—')
  })

  it('offers the cycle choice only when there is more than one cycle, and reports the pick', async () => {
    const both = mountDetail()
    await both.find('[data-testid="product-cycle-annual"]').trigger('click')
    expect(both.emitted('update:selectedCycle')?.[0]).toEqual([PricingCycle.Annual])

    const single = mountDetail({ cycles: [PricingCycle.Monthly] })
    expect(single.find('[data-testid="product-cycle-monthly"]').exists()).toBe(false)
  })

  it('reports the plan the visitor adds to the cart, and shows "in cart" for one already there', async () => {
    const wrapper = mountDetail({ isInCart: (plan: ProductPlan) => plan.pid === 12 })

    await wrapper.find('[data-testid="product-add-11"]').trigger('click')

    expect(wrapper.emitted('addToCart')?.[0]).toEqual([product.plans[0]])
    expect(wrapper.find('[data-testid="product-add-12"]').text()).toContain('hosting.inCart')
    expect(wrapper.find('[data-testid="product-add-11"]').text()).toContain('hosting.addToCart')
  })

  it('shows the trial button only for a plan that has an external order page', async () => {
    const wrapper = mountDetail({ trialUrlFor: (plan: ProductPlan) => (plan.pid === 11 ? 'https://orders.test/cart' : null) })

    expect(wrapper.find('[data-testid="product-trial-12"]').exists()).toBe(false)
    await wrapper.find('[data-testid="product-trial-11"]').trigger('click')
    expect(wrapper.emitted('trial')?.[0]).toEqual([product.plans[0]])
  })

  it('reports the demo request and links to the contact page', async () => {
    const wrapper = mountDetail()

    await wrapper.find('[data-testid="product-demo"]').trigger('click')

    expect(wrapper.emitted('demo')).toHaveLength(1)
    expect(wrapper.find('[data-testid="product-contact"]').attributes('href')).toBe('/en/contact')
  })
})

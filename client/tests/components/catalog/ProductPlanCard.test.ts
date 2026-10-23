/**
 * Unit tests for ProductPlanCard: the plan's name, short description, price with its period and
 * yearly saving, the feature list with its "+N more" line, the equal-height structure and the
 * cart and trial buttons.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import ProductPlanCard from '~/components/catalog/ProductPlanCard.vue'
import { PLAN_FEATURE_LIMIT } from '~/constants/planFeatureLimit'
import { PricingCycle } from '~/types/pricingcycle'
import type { ProductPlan } from '~/types/productplan'

/** A plan with a monthly and a yearly price and two feature lines. */
const basePlan: ProductPlan = {
  id: 11,
  pid: 11,
  tier: 'Starter',
  pricing: { monthly: 10, annual: 100 },
  description: 'For a first site',
  features: ['One site', 'Free SSL'],
  isFeatured: false,
}

/**
 * Mounts the card with sensible defaults.
 *
 * @param props Props that differ from the defaults.
 * @returns The wrapper.
 */
const mountCard = (props: Record<string, unknown> = {}) =>
  mountWithKit(ProductPlanCard, {
    props: {
      plan: basePlan,
      price: '$10.00',
      cycle: PricingCycle.Monthly,
      inCart: false,
      trialUrl: null,
      ...props,
    },
  })

describe('ProductPlanCard', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
  })

  it('shows the plan name, its short description and its feature lines', () => {
    const wrapper = mountCard()

    expect(wrapper.find('[data-testid="product-plan-name"]').element.tagName).toBe('H3')
    expect(wrapper.find('[data-testid="product-plan-name"]').text()).toBe('Starter')
    expect(wrapper.find('[data-testid="product-plan-description"]').text()).toBe('For a first site')
    expect(wrapper.find('[data-testid="product-plan-features"]').text()).toContain('One site')
    expect(wrapper.find('[data-testid="product-plan-features"]').text()).toContain('Free SSL')
    expect(wrapper.find('[data-testid="product-plan-more"]').exists()).toBe(false)
  })

  it('keeps the description box when the plan has no copy, so every price in a row lines up', () => {
    const description = mountCard({ plan: { ...basePlan, description: '' } }).find('[data-testid="product-plan-description"]')

    expect(description.exists()).toBe(true)
    expect(description.text()).toBe('')
    expect(description.classes()).toContain('min-h-[3rem]')
  })

  it('lists at most PLAN_FEATURE_LIMIT lines and counts the rest', () => {
    const many = Array.from({ length: PLAN_FEATURE_LIMIT + 3 }, (_, index) => `Line ${index}`)
    const wrapper = mountCard({ plan: { ...basePlan, features: many } })

    expect(wrapper.findAll('[data-testid="product-plan-features"] li')).toHaveLength(PLAN_FEATURE_LIMIT + 1)
    expect(wrapper.find('[data-testid="product-plan-features"]').text()).not.toContain(`Line ${PLAN_FEATURE_LIMIT}`)
    expect(wrapper.find('[data-testid="product-plan-more"]').text()).toContain('products.pricing.more')
  })

  it('honours a custom feature limit', () => {
    const wrapper = mountCard({ plan: { ...basePlan, features: ['A', 'B', 'C'] }, maxFeatures: 1 })

    expect(wrapper.findAll('[data-testid="product-plan-features"] li')).toHaveLength(2)
  })

  it('omits the feature list for a plan with no feature lines', () => {
    expect(mountCard({ plan: { ...basePlan, features: [] } }).find('[data-testid="product-plan-features"]').exists()).toBe(false)
  })

  it('shows the price with the period of the chosen cycle', () => {
    const monthly = mountCard()
    expect(monthly.find('[data-testid="product-plan-price"]').text()).toBe('$10.00')
    expect(monthly.find('[data-testid="product-plan-period"]').text()).toBe('products.pricing.period.monthly')

    const yearly = mountCard({ price: '$100.00', cycle: PricingCycle.Annual })
    expect(yearly.find('[data-testid="product-plan-period"]').text()).toBe('products.pricing.period.annually')
  })

  it('shows a dash and no period for a plan without a price in the chosen cycle', () => {
    const wrapper = mountCard({ price: '' })

    expect(wrapper.find('[data-testid="product-plan-price"]').text()).toBe('—')
    expect(wrapper.find('[data-testid="product-plan-period"]').exists()).toBe(false)
  })

  it('announces the yearly saving only on the yearly price and only when there is one', () => {
    expect(mountCard({ price: '$100.00', cycle: PricingCycle.Annual }).find('[data-testid="product-plan-saving"]').exists()).toBe(true)
    expect(mountCard().find('[data-testid="product-plan-saving"]').exists()).toBe(false)

    const noSaving = mountCard({ plan: { ...basePlan, pricing: { monthly: 10, annual: 120 } }, cycle: PricingCycle.Annual })
    expect(noSaving.find('[data-testid="product-plan-saving"]').exists()).toBe(false)
  })

  it('stretches to its row and pins the buttons to the bottom', () => {
    const wrapper = mountCard()
    const card = wrapper.find('.ui-card')
    const buttons = wrapper.find('[data-testid="product-add-11"]').element.parentElement

    expect(card.classes()).toEqual(expect.arrayContaining(['h-full', 'flex', 'flex-col']))
    expect(buttons?.className).toContain('mt-auto')
  })

  it('wears the recommended badge and frame only when it is the popular plan', () => {
    const popular = mountCard({ popular: true })
    expect(popular.find('[data-testid="product-plan-popular"]').text()).toContain('products.pricing.popular')
    expect(popular.find('.ui-card').classes()).toContain('!border-2')

    const plain = mountCard()
    expect(plain.find('[data-testid="product-plan-popular"]').exists()).toBe(false)
    expect(plain.find('.ui-card').classes()).not.toContain('!border-2')
  })

  it('reports the add-to-cart click and shows "in cart" for a plan already there', async () => {
    const wrapper = mountCard()
    await wrapper.find('[data-testid="product-add-11"]').trigger('click')

    expect(wrapper.emitted('addToCart')?.[0]).toEqual([basePlan])
    expect(wrapper.find('[data-testid="product-add-11"]').text()).toContain('hosting.addToCart')
    expect(mountCard({ inCart: true }).find('[data-testid="product-add-11"]').text()).toContain('hosting.inCart')
  })

  it('shows the trial button only with an external order page, and reports its click', async () => {
    expect(mountCard().find('[data-testid="product-trial-11"]').exists()).toBe(false)

    const wrapper = mountCard({ trialUrl: 'https://orders.test/cart' })
    await wrapper.find('[data-testid="product-trial-11"]').trigger('click')

    expect(wrapper.emitted('trial')?.[0]).toEqual([basePlan])
  })
})

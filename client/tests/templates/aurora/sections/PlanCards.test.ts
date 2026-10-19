/**
 * Unit tests for PlanCards: the free plan on its own, equal-height paid cards, the billing toggle,
 * the header switch, the empty state and the shared "included" fallback.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import PlanCards from '~/templates/aurora/sections/PlanCards.vue'
import { stubAuroraGlobals, PLANS } from '../auroraGlobals'

describe('PlanCards', () => {
  it('pulls the free plan out of the grid and keeps the paid ones in it', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(PlanCards, { props: { plans: PLANS } })

    expect(wrapper.find('[data-testid="plan-free"]').text()).toContain('Free')
    expect(wrapper.find('[data-testid="plan-card-3"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="plan-card-1"]').text()).toContain('Starter')
    expect(wrapper.find('[data-testid="plan-card-2"]').text()).toContain('Business')
  })

  it('links every plan button to its order route through the e2e ids', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(PlanCards, { props: { plans: PLANS } })

    expect(wrapper.find('[data-testid="plan-choose-1"]').attributes('href')).toBe('/configure/1')
    expect(wrapper.find('[data-testid="plan-choose-3"]').attributes('href')).toBe('/configure/3')
  })

  it('keeps the paid cards the same height and pins price and button to the bottom', () => {
    stubAuroraGlobals()
    const card = mountWithKit(PlanCards, { props: { plans: PLANS } }).find('[data-testid="plan-card-1"]')

    expect(card.classes()).toEqual(expect.arrayContaining(['h-full', 'flex', 'flex-col']))
    expect(card.find('.mt-auto').exists()).toBe(true)
  })

  it('shows the monthly price by default and the per-month annual price when yearly', () => {
    stubAuroraGlobals()

    expect(mountWithKit(PlanCards, { props: { plans: PLANS } }).find('[data-testid="plan-card-1"]').text()).toContain('$4.99')
    expect(mountWithKit(PlanCards, { props: { plans: PLANS, yearly: true } }).find('[data-testid="plan-card-1"]').text()).toContain('$3.99')
  })

  it('reports the billing period the visitor picks', async () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(PlanCards, { props: { plans: PLANS } })

    await wrapper.find('[data-testid="billing-yearly"]').trigger('click')
    await wrapper.find('[data-testid="billing-monthly"]').trigger('click')

    expect(wrapper.emitted('update:yearly')).toEqual([[true], [false]])
  })

  it('marks the active period and paints it with the brand gradient', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(PlanCards, { props: { plans: PLANS, yearly: true } })

    expect(wrapper.find('[data-testid="billing-yearly"]').attributes('aria-pressed')).toBe('true')
    expect(wrapper.find('[data-testid="billing-yearly"]').classes()).toContain('bg-brand')
    expect(wrapper.find('[data-testid="billing-monthly"]').classes()).not.toContain('bg-brand')
  })

  it('keeps the toggle track and both segments in one pill radius family with white text on the gradient', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(PlanCards, { props: { plans: PLANS, yearly: true } })
    const active = wrapper.find('[data-testid="billing-yearly"]').classes()
    const idle = wrapper.find('[data-testid="billing-monthly"]').classes()

    expect(wrapper.find('[data-testid="billing-toggle"]').classes()).toContain('rounded-full')
    expect(active).toEqual(expect.arrayContaining(['rounded-full', 'bg-brand', 'text-white', 'shadow-md', 'min-w-[120px]']))
    expect(idle).toEqual(expect.arrayContaining(['rounded-full', 'text-tx2', 'hover:text-tx', 'min-w-[120px]']))
    expect(active.some((c) => c.startsWith('focus:'))).toBe(false)
  })

  it('drops the section headline when the page already has one', () => {
    stubAuroraGlobals()

    expect(mountWithKit(PlanCards, { props: { plans: PLANS } }).find('h2').text()).toBe('aurora.plans.title')
    expect(mountWithKit(PlanCards, { props: { plans: PLANS, showHeader: false } }).find('h2').exists()).toBe(false)
  })

  it('says so when there are no plans', () => {
    stubAuroraGlobals()

    expect(mountWithKit(PlanCards).find('[data-testid="plans-empty"]').text()).toBe('aurora.plans.empty')
  })

  it('lists what every plan includes only when no plan carries its own features', () => {
    stubAuroraGlobals()
    const bare = PLANS.filter((plan) => plan.features.length === 0)

    expect(mountWithKit(PlanCards, { props: { plans: bare } }).find('[data-testid="plans-included"]').exists()).toBe(true)
    expect(mountWithKit(PlanCards, { props: { plans: PLANS } }).find('[data-testid="plans-included"]').exists()).toBe(false)
  })
})

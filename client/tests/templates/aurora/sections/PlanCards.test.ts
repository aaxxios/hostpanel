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

    const monthly = mountWithKit(PlanCards, { props: { plans: PLANS } })
    await monthly.find('[data-testid="billing-yearly"]').trigger('click')
    const yearly = mountWithKit(PlanCards, { props: { plans: PLANS, yearly: true } })
    await yearly.find('[data-testid="billing-monthly"]').trigger('click')

    expect(monthly.emitted('update:yearly')).toEqual([[true]])
    expect(yearly.emitted('update:yearly')).toEqual([[false]])
  })

  it('renders the toggle as the kit segmented control with the active period checked', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(PlanCards, { props: { plans: PLANS, yearly: true } })
    const active = wrapper.find('[data-testid="billing-yearly"]')

    expect(wrapper.find('[data-testid="billing-toggle"]').attributes('role')).toBe('radiogroup')
    expect(active.attributes('aria-checked')).toBe('true')
    expect(wrapper.find('[data-testid="billing-monthly"]').attributes('aria-checked')).toBe('false')
    expect(active.classes()).toEqual(expect.arrayContaining(['rounded-full', 'bg-brand', 'text-white']))
    expect(active.classes()).not.toContain('rounded-lg')
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

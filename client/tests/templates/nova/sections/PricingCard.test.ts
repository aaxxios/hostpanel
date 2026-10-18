/**
 * Unit tests for PricingCard: price per billing period, badges, saving line and the order link.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import PricingCard from '~/templates/nova/sections/PricingCard.vue'
import type { NovaPlan } from '~/templates/nova/types/novaplan'

/**
 * Builds a plan with sensible defaults.
 *
 * @param overrides Fields to change.
 * @returns A plan for the card.
 */
const plan = (overrides: Partial<NovaPlan> = {}): NovaPlan => ({
  id: 7,
  name: 'Starter',
  description: 'For one site',
  features: ['10 GB', '1 domain'],
  isFree: false,
  priceMonthly: '$5.00',
  priceAnnual: '$4.00',
  href: '/configure/7',
  popular: false,
  discountPercent: 20,
  ...overrides,
})

describe('PricingCard', () => {
  it('shows the monthly price and per-month label by default', () => {
    const wrapper = mountWithKit(PricingCard, { props: { plan: plan(), yearly: false } })

    expect(wrapper.text()).toContain('$5.00')
    expect(wrapper.text()).toContain('nova.plans.perMonth')
    expect(wrapper.text()).not.toContain('nova.plans.save')
  })

  it('shows the annual price and the saving on the yearly view', () => {
    const wrapper = mountWithKit(PricingCard, { props: { plan: plan(), yearly: true } })

    expect(wrapper.text()).toContain('$4.00')
    expect(wrapper.text()).toContain('nova.plans.perMonthAnnual')
    expect(wrapper.text()).toContain('nova.plans.save')
  })

  it('badges the popular plan and gives it the accent call to action', () => {
    const wrapper = mountWithKit(PricingCard, { props: { plan: plan({ popular: true }), yearly: false } })

    expect(wrapper.text()).toContain('nova.plans.popular')
    expect(wrapper.find('[data-testid="nova-plan-cta-7"]').classes()).toContain('bg-nova-accent')
  })

  it('states a free plan on its own terms, whatever the billing period', () => {
    const wrapper = mountWithKit(PricingCard, {
      props: { plan: plan({ isFree: true, priceMonthly: '$0.00', priceAnnual: '$0.00', discountPercent: null }), yearly: true },
    })

    expect(wrapper.text()).toContain('nova.plans.freeBadge')
    expect(wrapper.text()).toContain('nova.plans.freeForever')
    expect(wrapper.text()).not.toContain('nova.plans.perMonthAnnual')
  })

  it('links the order action and names the plan for screen readers', () => {
    const cta = mountWithKit(PricingCard, { props: { plan: plan(), yearly: false } }).find('[data-testid="nova-plan-cta-7"]')

    expect(cta.attributes('href')).toBe('/configure/7')
    expect(cta.find('.sr-only').text()).toContain('Starter')
  })

  it('lists the plan features and omits an empty list', () => {
    expect(mountWithKit(PricingCard, { props: { plan: plan(), yearly: false } }).findAll('li')).toHaveLength(2)
    expect(mountWithKit(PricingCard, { props: { plan: plan({ features: [] }), yearly: false } }).findAll('li')).toHaveLength(0)
  })
})

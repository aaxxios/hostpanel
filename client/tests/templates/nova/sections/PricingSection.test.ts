/**
 * Unit tests for PricingSection: the billing toggle, the plan cards and the empty state.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import PricingSection from '~/templates/nova/sections/PricingSection.vue'
import type { PlanCard } from '~/types/plancard'

/**
 * Builds a plan card as the catalogue store formats it.
 *
 * @param id Product id.
 * @returns A plan card.
 */
const card = (id: number): PlanCard => ({
  id,
  name: `Plan ${id}`,
  description: '',
  features: [],
  isFree: false,
  priceMonthly: `$${id}.00`,
  priceAnnual: `$${id}.00`,
  href: `/configure/${id}`,
})

describe('PricingSection', () => {
  it('renders one card per plan', () => {
    const wrapper = mountWithKit(PricingSection, { props: { plans: [card(1), card(2), card(3)] } })

    expect(wrapper.findAll('article')).toHaveLength(3)
    expect(wrapper.find('[data-testid="nova-plans-empty"]').exists()).toBe(false)
  })

  it('says so when the catalogue returned nothing', () => {
    const wrapper = mountWithKit(PricingSection, { props: { plans: [] } })

    expect(wrapper.find('[data-testid="nova-plans-empty"]').text()).toContain('nova.plans.empty')
  })

  it('marks the selected billing period as pressed', () => {
    const monthly = mountWithKit(PricingSection, { props: { yearly: false } })
    const yearly = mountWithKit(PricingSection, { props: { yearly: true } })

    expect(monthly.find('[data-testid="nova-billing-monthly"]').attributes('aria-pressed')).toBe('true')
    expect(yearly.find('[data-testid="nova-billing-yearly"]').attributes('aria-pressed')).toBe('true')
  })

  it('reports the chosen period upward', async () => {
    const wrapper = mountWithKit(PricingSection, { props: { yearly: false } })

    await wrapper.find('[data-testid="nova-billing-yearly"]').trigger('click')
    await wrapper.find('[data-testid="nova-billing-monthly"]').trigger('click')

    expect(wrapper.emitted('update:yearly')).toEqual([[true], [false]])
  })

  it('leaves the heading out when the page supplies its own', () => {
    expect(mountWithKit(PricingSection, { props: { showHeader: true } }).find('h2').exists()).toBe(true)
    expect(mountWithKit(PricingSection, { props: { showHeader: false } }).find('h2').exists()).toBe(false)
  })
})

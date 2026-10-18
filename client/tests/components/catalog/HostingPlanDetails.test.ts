/**
 * Unit tests for HostingPlanDetails: the cycle choice, the included features and the closing order button.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import HostingPlanDetails from '~/components/catalog/HostingPlanDetails.vue'
import { PricingCycle } from '~/types/pricingcycle'

/**
 * Mounts the details.
 *
 * @param props Props that differ from the defaults.
 * @returns The wrapper.
 */
const mountDetails = (props: Record<string, unknown> = {}) =>
  mountWithKit(HostingPlanDetails, {
    props: {
      cycles: [
        { cycle: PricingCycle.Monthly, label: 'Monthly', price: '$5.00' },
        { cycle: PricingCycle.Annual, label: 'Annually', price: '$50.00' },
      ],
      selectedCycle: PricingCycle.Monthly,
      features: ['600 MB SSD', 'Free SSL'],
      inCart: false,
      ...props,
    },
  })

describe('HostingPlanDetails', () => {
  it('lists the cycles with their prices and reports the pick', async () => {
    const wrapper = mountDetails()

    expect(wrapper.find('[data-testid="hosting-plan-cycle-monthly"]').text()).toContain('$5.00')
    await wrapper.find('[data-testid="hosting-plan-cycle-annual"]').trigger('click')

    expect(wrapper.emitted('update:selectedCycle')?.[0]).toEqual([PricingCycle.Annual])
  })

  it('hides the cycle card when the plan has no priced cycle', () => {
    expect(mountDetails({ cycles: [] }).find('[data-testid="hosting-plan-cycles"]').exists()).toBe(false)
  })

  it('lists the included features, and hides the card when there are none', () => {
    expect(mountDetails().find('[data-testid="hosting-plan-features"]').text()).toContain('Free SSL')
    expect(mountDetails({ features: [] }).find('[data-testid="hosting-plan-features"]').exists()).toBe(false)
  })

  it('reports the closing order button and labels it by the cart state', async () => {
    const wrapper = mountDetails()
    await wrapper.find('[data-testid="hosting-plan-order-bottom"]').trigger('click')

    expect(wrapper.emitted('order')).toHaveLength(1)
    expect(wrapper.find('[data-testid="hosting-plan-order-bottom"]').text()).toContain('hosting.orderNow')
    expect(mountDetails({ inCart: true }).find('[data-testid="hosting-plan-order-bottom"]').text()).toContain('hosting.inCart')
  })

  it('shows the secure-checkout line', () => {
    expect(mountDetails().text()).toContain('cart.secureCheckout')
  })
})

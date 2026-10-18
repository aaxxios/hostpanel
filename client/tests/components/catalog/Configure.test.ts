/**
 * Unit tests for Configure: the skeleton, the not-found notice, the product details, the cycle
 * choice and the order summary with its Continue button.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import Configure from '~/components/catalog/Configure.vue'
import { PricingCycle } from '~/types/pricingcycle'

/**
 * Mounts the screen.
 *
 * @param props Props that differ from the defaults.
 * @returns The wrapper.
 */
const mountConfigure = (props: Record<string, unknown> = {}) =>
  mountWithKit(Configure, {
    props: {
      pending: false,
      found: true,
      groupName: 'Web Hosting',
      productName: 'Starter',
      description: 'Small sites',
      features: ['5 GB SSD', '10 Emails'],
      icon: 'cloud-check',
      color: '#0ea5e9',
      cycles: [
        { cycle: PricingCycle.Monthly, label: 'Monthly', price: '$5.00' },
        { cycle: PricingCycle.Annual, label: 'Annually', price: '$50.00' },
      ],
      selectedCycle: PricingCycle.Monthly,
      selectedCycleLabel: 'Monthly',
      selectedPrice: '$5.00',
      setupFee: '$0.00',
      ...props,
    },
  })

describe('Configure', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
  })

  it('shows a skeleton while the product is read', () => {
    const wrapper = mountConfigure({ pending: true, found: false })

    expect(wrapper.find('[data-testid="configure-loading"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="configure-summary"]').exists()).toBe(false)
  })

  it('shows a notice with the way back when the product does not exist', () => {
    const wrapper = mountConfigure({ found: false })

    expect(wrapper.find('[data-testid="configure-missing"]').text()).toContain('configure.notFound')
    expect(wrapper.find('[data-testid="configure-missing-back"]').attributes('href')).toBe('/en/products')
  })

  it('shows the product name, its family and its description', () => {
    const wrapper = mountConfigure()

    expect(wrapper.find('[data-testid="configure-product-name"]').element.tagName).toBe('H1')
    expect(wrapper.find('[data-testid="configure-product-name"]').text()).toBe('Starter')
    expect(wrapper.text()).toContain('Web Hosting')
    expect(wrapper.text()).toContain('Small sites')
  })

  it('lists the features, and hides the card when there are none', () => {
    expect(mountConfigure().find('[data-testid="configure-features"]').text()).toContain('10 Emails')
    expect(mountConfigure({ features: [] }).find('[data-testid="configure-features"]').exists()).toBe(false)
  })

  it('draws the family mark only when the family is known', () => {
    expect(mountConfigure().find('[data-testid="ui-accent-tile"]').exists()).toBe(true)
    expect(mountConfigure({ icon: null, color: null }).find('[data-testid="ui-accent-tile"]').exists()).toBe(false)
  })

  it('reports the cycle the visitor picks, from the row or its radio', async () => {
    const wrapper = mountConfigure()

    await wrapper.find('[data-testid="configure-cycle-annual"]').trigger('click')

    expect(wrapper.emitted('update:selectedCycle')?.[0]).toEqual([PricingCycle.Annual])
  })

  it('shows the totals in the order summary', () => {
    const summary = mountConfigure().find('[data-testid="configure-summary"]')

    expect(summary.text()).toContain('configure.setupFees')
    expect(summary.text()).toContain('$0.00')
    expect(summary.find('[data-testid="configure-total"]').text()).toBe('$5.00')
  })

  it('reports Continue', async () => {
    const wrapper = mountConfigure()

    await wrapper.find('[data-testid="configure-continue"]').trigger('click')

    expect(wrapper.emitted('continue')).toHaveLength(1)
  })
})

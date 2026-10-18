/**
 * Tests for `pages/client/order-success.vue` against the real client store: the order summary
 * read from the link and the refresh before opening the services page. Only the API composable
 * is replaced.
 *
 * @module tests/pages/client/order-success.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountAuthPage, stubAuthRuntime } from '~/tests/helpers/authScreens'

/** The mocked services call, re-created for every test. */
const fetchServices = vi.fn()

vi.mock('~/composables/apis/useClientApi', () => ({
  /**
   * Stands in for the real API composable so the client store runs without transport.
   *
   * @returns Only the endpoint this page reaches through the store.
   */
  useClientApi: () => ({ fetchServices })
}))

const { default: OrderSuccessPage } = await import('~/pages/client/order-success.vue')

beforeEach(() => {
  fetchServices.mockReset()
  fetchServices.mockResolvedValue([])
})

describe('client order-success page', () => {
  it('shows the order number and amount from the link', async () => {
    stubAuthRuntime({ query: { order: 'A-1001', amount: '$25.00' } })

    const wrapper = await mountAuthPage(OrderSuccessPage)

    expect(wrapper.find('[data-testid="order-success-number"]').text()).toContain('A-1001')
    expect(wrapper.find('[data-testid="order-success-amount"]').text()).toContain('$25.00')
  })

  it('shows dashes when the link carries no order and a zero amount', async () => {
    stubAuthRuntime({ query: { amount: '0.00' } })

    const wrapper = await mountAuthPage(OrderSuccessPage)

    expect(wrapper.find('[data-testid="order-success-number"]').text()).toContain('—')
    expect(wrapper.find('[data-testid="order-success-amount"]').text()).toContain('—')
  })

  it('hides the domain rows and the manage-domain button for a hosting-only order', async () => {
    stubAuthRuntime({ query: { order: '1' } })

    const wrapper = await mountAuthPage(OrderSuccessPage)

    expect(wrapper.find('[data-testid="order-success-domain"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="order-success-action"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="order-success-manage-domain"]').exists()).toBe(false)
  })

  it('shows the domain, its action and the manage-domain button when a domain was bought', async () => {
    stubAuthRuntime({ query: { order: '1', domain: 'example.com', action: 'transfer' } })

    const wrapper = await mountAuthPage(OrderSuccessPage)

    expect(wrapper.find('[data-testid="order-success-domain"]').text()).toContain('example.com')
    expect(wrapper.find('[data-testid="order-success-action"]').text()).toContain('checkout.success.transfer')
    expect(wrapper.find('[data-testid="order-success-manage-domain"]').attributes('href')).toBe('/client/domains')
  })

  it('names a registration as such', async () => {
    stubAuthRuntime({ query: { domain: 'example.com', action: 'register' } })

    const wrapper = await mountAuthPage(OrderSuccessPage)

    expect(wrapper.find('[data-testid="order-success-action"]').text()).toContain('checkout.success.registration')
  })

  it('links to the invoices', async () => {
    stubAuthRuntime()

    const wrapper = await mountAuthPage(OrderSuccessPage)

    expect(wrapper.find('[data-testid="order-success-invoices"]').attributes('href')).toBe('/client/invoices')
  })

  it('reloads the services before opening the services page', async () => {
    stubAuthRuntime()
    const wrapper = await mountAuthPage(OrderSuccessPage)

    await wrapper.find('[data-testid="order-success-setup-service"]').trigger('click')
    await flushPromises()

    expect(fetchServices).toHaveBeenCalledTimes(1)
    expect(navigateTo).toHaveBeenCalledWith('/client/services')
  })
})

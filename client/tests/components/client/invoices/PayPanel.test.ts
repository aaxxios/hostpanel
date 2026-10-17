/**
 * Unit tests for PayPanel: which ways to pay are offered, the saved-card payment through the
 * store and the success and refusal states.
 */
import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountClient, stubClientRuntime } from '~/tests/helpers/mountClient'
import PayPanel from '~/components/client/invoices/PayPanel.vue'
import { useInvoicePaymentStore } from '~/stores/invoicepayment'
import type { PaymentMethod } from '~/types/payment'

/** The mocked billing endpoints the store reaches. */
const billingApi = vi.hoisted(() => ({ payInvoice: vi.fn(), startInvoiceGatewayPayment: vi.fn() }))

vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => billingApi }))

/** A saved card. */
const card = { id: 'pm_1', type: 'card', description: 'Visa', gateway_name: 'stripe', card_type: 'visa', card_last_four: '4242' } as PaymentMethod

/**
 * Mounts the panel after filling the store the page would have filled.
 *
 * @param state - Store fields to set (saved methods, gateways).
 * @param billingPortalUrl - The billing portal the deployment fronts, or an empty string.
 * @returns The wrapper.
 */
const mountPanel = (state: Record<string, unknown> = {}, billingPortalUrl = '') => {
  vi.stubGlobal('useRuntimeConfig', () => ({ public: { billingPortalUrl } }))
  const pinia = createPinia()
  setActivePinia(pinia)
  Object.assign(useInvoicePaymentStore(), state)
  return mountClient(PayPanel, { props: { invoiceId: '9' }, pinia })
}

describe('PayPanel', () => {
  beforeEach(() => {
    billingApi.payInvoice.mockReset()
    billingApi.startInvoiceGatewayPayment.mockReset()
    stubClientRuntime()
  })

  it('preselects the first saved card and offers to submit', () => {
    const wrapper = mountPanel({ savedMethods: [card] })

    expect(wrapper.find('[data-testid="pay-method-pm_1"]').attributes('aria-checked')).toBe('true')
    expect(wrapper.find('[data-testid="invoice-pay-submit"]').exists()).toBe(true)
  })

  it('pays with the saved card and shows the success state', async () => {
    billingApi.payInvoice.mockResolvedValue({})
    const wrapper = mountPanel({ savedMethods: [card] })

    await wrapper.find('[data-testid="invoice-pay-submit"]').trigger('click')

    await vi.waitFor(() => expect(wrapper.find('[data-testid="invoice-pay-success"]').exists()).toBe(true))
    expect(billingApi.payInvoice).toHaveBeenCalledWith('9', 'pm_1')
    expect(wrapper.find('[data-testid="invoice-pay-submit"]').exists()).toBe(false)
  })

  it('shows the refusal and keeps the button when the payment fails', async () => {
    billingApi.payInvoice.mockRejectedValue({ data: { detail: 'Card declined.' } })
    const wrapper = mountPanel({ savedMethods: [card] })

    await wrapper.find('[data-testid="invoice-pay-submit"]').trigger('click')

    await vi.waitFor(() => expect(wrapper.find('[data-testid="invoice-pay-error"]').text()).toBe('Card declined.'))
    expect(wrapper.find('[data-testid="invoice-pay-submit"]').exists()).toBe(true)
  })

  it('offers nothing to pay with when there is no saved card and no billing portal', () => {
    const wrapper = mountPanel()

    expect(wrapper.find('[data-testid="pay-method-selector"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="invoice-pay-submit"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="pay-card-form"]').exists()).toBe(false)
  })

  it('shows the card form when the billing portal can take a new card', () => {
    const wrapper = mountPanel({}, 'https://billing.example.net')

    expect(wrapper.find('[data-testid="pay-card-form"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="invoice-pay-submit"]').exists()).toBe(true)
  })

  it('offers the hosted gateway button when a gateway plugin is enabled', () => {
    const wrapper = mountPanel({ gatewayMethods: [{ module: 'hostedbank', name: 'Bank' }] })

    expect(wrapper.find('[data-testid="invoice-pay-hosted"]').exists()).toBe(true)
  })
})

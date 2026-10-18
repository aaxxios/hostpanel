/**
 * Unit tests for the payment result page: it verifies a hosted-gateway payment from the address and
 * shows the paid, pending, declined, missing or unknown outcome, never asserting money moved unless
 * the bank said so.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountPage, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import ResultPage from '~/pages/payment/result.vue'

/** The mocked billing endpoints, re-created for every test. */
const billing = { completeGatewayPayment: vi.fn() }

vi.mock('~/composables/apis/useBillingApi', () => ({
  /**
   * Stands in for the billing API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useBillingApi: () => billing,
}))

/** The order payment token helpers of the current test. */
const tokens = { recall: vi.fn(), forget: vi.fn() }

/**
 * Mounts the page for one address query and lets the check finish.
 *
 * @param query The address query string.
 * @returns The wrapper.
 */
const mountResult = async (query: Record<string, string> = {}) => {
  stubPublicPageRuntime({ query })
  const wrapper = await mountPage(ResultPage)
  await flushPromises()
  return wrapper
}

describe('payment result page', () => {
  beforeEach(() => {
    billing.completeGatewayPayment.mockReset()
    tokens.recall.mockReset()
    tokens.recall.mockReturnValue('token-1')
    tokens.forget.mockReset()
    vi.stubGlobal('useOrderPaymentToken', () => tokens)
  })

  it('says there is nothing to check, and does not call the backend, when the address names no payment', async () => {
    const wrapper = await mountResult()

    expect(wrapper.find('[data-testid="payment-title"]').text()).toBe('paymentResult.missingTitle')
    expect(wrapper.find('[data-testid="payment-invoices"]').attributes('href')).toBe('/en/client/invoices')
    expect(billing.completeGatewayPayment).not.toHaveBeenCalled()
  })

  it('verifies an order with its payment token and offers the order page once paid', async () => {
    billing.completeGatewayPayment.mockResolvedValue({ state: 'paid' })

    const wrapper = await mountResult({ order: '5' })

    expect(billing.completeGatewayPayment).toHaveBeenCalledWith('order', '5', 'token-1')
    expect(wrapper.find('[data-testid="payment-title"]').text()).toBe('paymentResult.successTitle')
    expect(wrapper.find('[data-testid="payment-continue"]').attributes('href')).toBe('/en/client/order-success?order=ORD-0005')
    expect(tokens.forget).toHaveBeenCalledWith('5')
  })

  it('verifies an invoice without a token and offers the invoice once paid', async () => {
    billing.completeGatewayPayment.mockResolvedValue({ state: 'paid' })

    const wrapper = await mountResult({ invoice: '9' })

    expect(billing.completeGatewayPayment).toHaveBeenCalledWith('invoice', '9', undefined)
    expect(wrapper.find('[data-testid="payment-continue"]').attributes('href')).toBe('/en/client/invoices/9')
  })

  it('keeps the token and lets the payer check again while the bank has not settled', async () => {
    billing.completeGatewayPayment.mockResolvedValue({ state: 'pending' })
    const wrapper = await mountResult({ order: '5' })
    expect(wrapper.find('[data-testid="payment-title"]').text()).toBe('paymentResult.pendingTitle')
    expect(tokens.forget).not.toHaveBeenCalled()

    billing.completeGatewayPayment.mockResolvedValue({ state: 'paid' })
    await wrapper.find('[data-testid="payment-retry"]').trigger('click')
    await flushPromises()

    expect(billing.completeGatewayPayment).toHaveBeenCalledTimes(2)
    expect(wrapper.find('[data-testid="payment-title"]').text()).toBe('paymentResult.successTitle')
  })

  it('sends a declined order payer back to the cart, and a declined invoice payer to the invoice payment', async () => {
    billing.completeGatewayPayment.mockResolvedValue({ state: 'declined' })

    const order = await mountResult({ order: '5' })
    expect(order.find('[data-testid="payment-try-again"]').attributes('href')).toBe('/en/cart')

    const invoice = await mountResult({ invoice: '9' })
    expect(invoice.find('[data-testid="payment-try-again"]').attributes('href')).toBe('/en/client/invoices/9/pay')
  })

  it('does not assert an outcome when the backend cannot be reached, and keeps the token', async () => {
    billing.completeGatewayPayment.mockRejectedValue(new Error('offline'))

    const wrapper = await mountResult({ order: '5' })

    expect(wrapper.find('[data-testid="payment-title"]').text()).toBe('paymentResult.unknownTitle')
    expect(wrapper.find('[data-testid="payment-status"]').exists()).toBe(true)
    expect(tokens.forget).not.toHaveBeenCalled()
  })
})

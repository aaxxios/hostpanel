/**
 * Tests for `pages/client/invoices/[id]/pay.vue`: the layout, the already-paid and not-found
 * states and the saved-card payment.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountPage, stubClientRuntime } from '~/tests/helpers/mountClient'
import PayPage from '~/pages/client/invoices/[id]/pay.vue'
import { InvoiceStatus } from '~/types/invoicestatus'

/** The mocked billing endpoints the store reaches. */
const billingApi = vi.hoisted(() => ({
  fetchInvoice: vi.fn(), fetchPaymentMethods: vi.fn(), fetchGatewayMethods: vi.fn(), payInvoice: vi.fn()
}))

vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => billingApi }))

/**
 * Builds an invoice.
 *
 * @param status - Invoice status.
 * @returns The invoice.
 */
const invoice = (status: InvoiceStatus) => ({
  id: 41, status, currency: 'USD', subTotal: 100, credit: 0, total: 100, items: [{ id: 1, description: 'Hosting', amount: 100 }], transactions: []
})

/** A saved card. */
const card = { id: 'pm_1', type: 'card', description: 'Visa', gateway_name: 'stripe', card_type: 'visa', card_last_four: '4242' }

describe('invoice payment page', () => {
  beforeEach(() => {
    Object.values(billingApi).forEach((fn) => fn.mockReset())
    billingApi.fetchPaymentMethods.mockResolvedValue([card])
    billingApi.fetchGatewayMethods.mockResolvedValue([])
    stubClientRuntime({ params: { id: '41' } })
  })

  it('shows the payment panel beside the invoice summary', async () => {
    billingApi.fetchInvoice.mockResolvedValue(invoice(InvoiceStatus.Unpaid))

    const wrapper = await mountPage(PayPage)

    expect(wrapper.find('[data-testid="invoice-pay-panel"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="pay-summary"]').text()).toContain('Hosting')
    expect(wrapper.find('[data-testid="invoice-pay-security"]').exists()).toBe(true)
  })

  it('says an already paid invoice has been paid and offers no payment', async () => {
    billingApi.fetchInvoice.mockResolvedValue(invoice(InvoiceStatus.Paid))

    const wrapper = await mountPage(PayPage)

    expect(wrapper.find('[data-testid="invoice-already-paid"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="invoice-pay-panel"]').exists()).toBe(false)
  })

  it('says the invoice was not found when the read fails', async () => {
    billingApi.fetchInvoice.mockRejectedValue({ data: { detail: 'No such invoice.' } })

    const wrapper = await mountPage(PayPage)

    expect(wrapper.text()).toContain('client.invoices.notFound')
    expect(wrapper.find('[data-testid="invoice-pay-panel"]').exists()).toBe(false)
  })

  it('pays with the preselected saved card', async () => {
    billingApi.fetchInvoice.mockResolvedValue(invoice(InvoiceStatus.Unpaid))
    billingApi.payInvoice.mockResolvedValue({})
    const wrapper = await mountPage(PayPage)

    await wrapper.find('[data-testid="invoice-pay-submit"]').trigger('click')

    await vi.waitFor(() => expect(wrapper.find('[data-testid="invoice-pay-success"]').exists()).toBe(true))
    expect(billingApi.payInvoice).toHaveBeenCalledWith('41', 'pm_1')
  })

  it('links the breadcrumb back to the invoice', async () => {
    billingApi.fetchInvoice.mockResolvedValue(invoice(InvoiceStatus.Unpaid))

    const wrapper = await mountPage(PayPage)

    expect(wrapper.find('nav a[href="/client/invoices/41"]').exists()).toBe(true)
  })
})

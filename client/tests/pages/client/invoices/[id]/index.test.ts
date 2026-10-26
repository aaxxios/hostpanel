/**
 * Tests for `pages/client/invoices/[id]/index.vue`: the header, the pay button of an owing invoice, the
 * totals and the not-found state.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountPage, stubClientRuntime } from '~/tests/helpers/mountClient'
import InvoicePage from '~/pages/client/invoices/[id]/index.vue'
import { InvoiceStatus } from '~/types/invoicestatus'

/** The mocked billing endpoints the store reaches. */
const billingApi = vi.hoisted(() => ({
  fetchInvoice: vi.fn(),
  invoicePdfUrl: (id: string) => `/api/portal/client/invoices/${id}/pdf`,
}))

vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => billingApi }))

/**
 * Builds an invoice.
 *
 * @param overrides - Fields to set.
 * @returns The invoice.
 */
const invoice = (overrides: Record<string, unknown> = {}) => ({
  id: 41, status: InvoiceStatus.Unpaid, currency: 'USD', invoiceDate: '2026-01-01', dueDate: '2099-01-15', paidAt: null, paymentMethod: 'card',
  subTotal: 90, tax: 10, credit: 0, total: 100,
  items: [{ id: 1, description: 'Hosting', amount: 90 }], transactions: [],
  ...overrides
})

describe('invoice page', () => {
  beforeEach(() => {
    billingApi.fetchInvoice.mockReset()
    stubClientRuntime({ params: { id: '41' } })
  })

  it('reads this invoice and shows its heading and line items', async () => {
    billingApi.fetchInvoice.mockResolvedValue(invoice())

    const wrapper = await mountPage(InvoicePage)

    expect(billingApi.fetchInvoice).toHaveBeenCalledWith('41')
    expect(wrapper.find('[data-testid="invoice-heading"]').text()).toContain('client.invoices.heading')
    expect(wrapper.find('[data-testid="invoice-items"]').text()).toContain('Hosting')
  })

  it('shows the amounts in the invoice currency, with tax only when there is some', async () => {
    billingApi.fetchInvoice.mockResolvedValue(invoice())
    const taxed = await mountPage(InvoicePage)
    billingApi.fetchInvoice.mockResolvedValue(invoice({ tax: 0, total: 90 }))
    const untaxed = await mountPage(InvoicePage)

    expect(taxed.find('[data-testid="invoice-items"]').text()).toContain('$100.00')
    expect(taxed.find('[data-testid="invoice-items"]').text()).toContain('client.invoices.tax')
    expect(untaxed.find('[data-testid="invoice-items"]').text()).not.toContain('client.invoices.tax')
  })

  it('offers payment for an owing invoice', async () => {
    billingApi.fetchInvoice.mockResolvedValue(invoice())

    const wrapper = await mountPage(InvoicePage)

    expect(wrapper.find('[data-testid="invoice-pay-now"]').attributes('href')).toBe('/client/invoices/41/pay')
    expect(wrapper.find('[data-testid="invoice-unpaid-banner"]').exists()).toBe(true)
  })

  it('offers no payment for a paid invoice', async () => {
    billingApi.fetchInvoice.mockResolvedValue(invoice({ status: InvoiceStatus.Paid, paidAt: '2026-01-05T10:00:00Z' }))

    const wrapper = await mountPage(InvoicePage)

    expect(wrapper.find('[data-testid="invoice-pay-now"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="invoice-unpaid-banner"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('client.invoices.datePaid')
  })

  it('says the invoice was not found when the read fails', async () => {
    billingApi.fetchInvoice.mockRejectedValue({ data: { detail: 'No such invoice.' } })

    const wrapper = await mountPage(InvoicePage)

    expect(wrapper.find('[data-testid="ui-empty-state"]').text()).toContain('client.invoices.notFound')
  })

  it('offers the invoice as a PDF download', async () => {
    billingApi.fetchInvoice.mockResolvedValue(invoice())

    const wrapper = await mountPage(InvoicePage)

    const link = wrapper.find('[data-testid="invoice-download"]')
    expect(link.exists()).toBe(true)
    expect(link.attributes('href')).toBe('/api/portal/client/invoices/41/pdf')
    expect(link.attributes('download')).toBeDefined()
    expect(link.text()).toContain('client.invoices.downloadPdf')
  })

  it('offers no PDF download for a draft', async () => {
    billingApi.fetchInvoice.mockResolvedValue(invoice({ status: InvoiceStatus.Draft }))

    const wrapper = await mountPage(InvoicePage)

    expect(wrapper.find('[data-testid="invoice-header"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="invoice-download"]').exists()).toBe(false)
  })
})

/**
 * Mount tests for InvoiceDetailView: the invoice loads by id into the tabs, the Options tab
 * exists only while managing, and a refused load shows the API sentence.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import InvoiceDetailView from '@/views/InvoiceDetailView.vue'
import { invoiceFixture } from '../helpers/billingFixtures'
import { json, problem } from '../helpers/fetch'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'

/** Route pattern of the screen, with the optional mode segment. */
const PATTERN = '/billing/:id/:action?'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('InvoiceDetailView', () => {
  it('loads the invoice and shows its number and the summary tab', async () => {
    stubRoutes({ 'GET /api/billing/1': () => json(invoiceFixture()) })
    const { wrapper } = await mountRouted(InvoiceDetailView, PATTERN, '/billing/1')
    await flushPromises()

    expect(wrapper.get('[data-testid="invoice-detail-title"]').text()).toContain('Invoice #1')
    expect(wrapper.find('[data-testid="billing-invoice-summary"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="billing-invoice-mark-paid"]').exists()).toBe(false)
  })

  it('shows the Options tab and the actions only after switching to manage mode', async () => {
    stubRoutes({ 'GET /api/billing/1': () => json(invoiceFixture()) })
    const { wrapper } = await mountRouted(InvoiceDetailView, PATTERN, '/billing/1')
    await flushPromises()
    expect(wrapper.find('[data-testid="billing-invoice-tab-options"]').exists()).toBe(false)

    await wrapper.get('[data-testid="billing-invoice-toggle-mode"]').trigger('click')

    expect(wrapper.find('[data-testid="billing-invoice-tab-options"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="billing-invoice-mark-paid"]').exists()).toBe(true)
  })

  it('opens in manage mode on the edit route and switches tabs', async () => {
    stubRoutes({ 'GET /api/billing/1': () => json(invoiceFixture()) })
    const { wrapper } = await mountRouted(InvoiceDetailView, PATTERN, '/billing/1/edit')
    await flushPromises()

    await wrapper.get('[data-testid="billing-invoice-tab-notes"]').trigger('click')

    expect(wrapper.find('[data-testid="billing-invoice-notes"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="billing-invoice-notes-input"]').exists()).toBe(true)
  })

  it('shows the API sentence when the invoice cannot be loaded', async () => {
    stubRoutes({ 'GET /api/billing/1': () => problem('Invoice not found.', 404) })
    const { wrapper } = await mountRouted(InvoiceDetailView, PATTERN, '/billing/1')
    await flushPromises()

    expect(wrapper.get('[data-testid="billing-invoice-error"]').text()).toContain('Invoice not found.')
    expect(wrapper.find('[data-testid="billing-invoice-summary"]').exists()).toBe(false)
  })
})

/**
 * Mount tests for InvoicesListView: the table renders what the API sent, ticking rows opens the
 * bulk bar, a bulk action goes through the store, and a refused load shows the API sentence.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import InvoicesListView from '@/views/InvoicesListView.vue'
import { invoiceFixture } from '../helpers/billingFixtures'
import { json, problem } from '../helpers/fetch'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'

/**
 * Mounts the list and waits for its first load.
 *
 * @returns The wrapper.
 */
const mountList = async () => {
  const { wrapper } = await mountRouted(InvoicesListView, '/billing/invoices', '/billing/invoices')
  await flushPromises()
  return wrapper
}

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('InvoicesListView', () => {
  it('renders a row per invoice with its client, status and links', async () => {
    stubRoutes({ 'GET /api/billing?': () => json({ items: [invoiceFixture()], totalCount: 1, page: 1, pageSize: 25 }) })
    const wrapper = await mountList()

    const rows = wrapper.findAll('[data-testid="billing-invoices-table"] tbody tr')
    expect(rows).toHaveLength(1)
    expect(rows[0]?.text()).toContain('Ada Lovelace')
    expect(rows[0]?.text()).toContain('Unpaid')
    expect(wrapper.get('[data-testid="billing-invoice-view-1"]').attributes('href')).toBe('/billing/invoices/1/view')
  })

  it('opens the bulk bar when a row is ticked and runs the action', async () => {
    const mock = stubRoutes({
      'GET /api/billing?': () => json({ items: [invoiceFixture()], totalCount: 1, page: 1, pageSize: 25 }),
      'POST /api/billing/bulk': () => new Response(null, { status: 204 }),
    })
    const wrapper = await mountList()
    expect(wrapper.find('[data-testid="billing-invoices-bulk"]').exists()).toBe(false)

    await wrapper.get('[data-testid="billing-invoice-select-1"]').trigger('click')
    await wrapper.get('[data-testid="billing-invoices-bulk-paid"]').trigger('click')
    await flushPromises()

    const post = mock.mock.calls.find(([url]) => String(url).endsWith('/billing/bulk'))
    expect(JSON.parse(String((post?.[1] as RequestInit | undefined)?.body))).toEqual({ invoiceIds: [1], action: 'MarkPaid' })
  })

  it('deletes an invoice only after the confirmation', async () => {
    const mock = stubRoutes({
      'GET /api/billing?': () => json({ items: [invoiceFixture()], totalCount: 1, page: 1, pageSize: 25 }),
      'DELETE /api/billing/1': () => new Response(null, { status: 204 }),
    })
    const wrapper = await mountList()

    await wrapper.get('[data-testid="billing-invoice-delete-1"]').trigger('click')
    expect(mock.mock.calls.some(([, init]) => (init as RequestInit | undefined)?.method === 'DELETE')).toBe(false)
    await wrapper.get('[data-testid="dialog-confirm"]').trigger('click')
    await flushPromises()

    expect(mock.mock.calls.some(([, init]) => (init as RequestInit | undefined)?.method === 'DELETE')).toBe(true)
  })

  it('shows the API sentence when the load is refused', async () => {
    stubRoutes({ 'GET /api/billing?': () => problem('Billing is disabled.', 403) })
    const wrapper = await mountList()

    expect(wrapper.get('[data-testid="billing-invoices-error"]').text()).toContain('Billing is disabled.')
  })
})

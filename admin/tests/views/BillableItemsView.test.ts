/**
 * Mount tests for BillableItemsView: the table renders what the API sent, the description filter
 * narrows it, a bulk delete asks first, and a refused load shows the API sentence.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import BillableItemsView from '@/views/BillableItemsView.vue'
import { json, problem } from '../helpers/fetch'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'

/**
 * Builds a billable item as the list endpoint returns it.
 *
 * @param overrides - Fields to replace.
 * @returns The item.
 */
const item = (overrides: Record<string, unknown> = {}) => ({
  id: 5,
  clientId: 7,
  clientName: 'Ada Lovelace',
  description: 'Domain renewal',
  amount: 12,
  currency: 'USD',
  type: 'OneTime',
  recurringPeriod: null,
  isInvoiced: false,
  invoiceId: null,
  nextDueDate: null,
  createdAt: '2026-10-01T00:00:00Z',
  ...overrides,
})

/** The items endpoint answer: a domain charge and a support charge. */
const itemsPage = () =>
  json({ items: [item(), item({ id: 6, description: 'Support hours' })], totalCount: 2, page: 1, pageSize: 100 })

/** The clients endpoint answer: nobody (the filter is not under test). */
const noClients = () => json({ items: [], totalCount: 0 })

/**
 * Mounts the list and waits for its first load.
 *
 * @returns The wrapper.
 */
const mountList = async () => {
  const { wrapper } = await mountRouted(BillableItemsView, '/billing/billable-items', '/billing/billable-items')
  await flushPromises()
  return wrapper
}

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('BillableItemsView', () => {
  it('renders a row per item with the amount and invoiced state', async () => {
    stubRoutes({ 'GET /api/billing/billable-items': itemsPage, 'GET /api/clients': noClients })
    const wrapper = await mountList()

    const rows = wrapper.findAll('[data-testid="billing-billable-items-table"] tbody tr')
    expect(rows).toHaveLength(2)
    expect(rows[0]?.text()).toContain('USD 12.00')
    expect(rows[0]?.text()).toContain('No')
  })

  it('narrows the items by description', async () => {
    stubRoutes({ 'GET /api/billing/billable-items': itemsPage, 'GET /api/clients': noClients })
    const wrapper = await mountList()

    await wrapper.get('[data-testid="billing-billable-items-filter-toggle"]').trigger('click')
    await wrapper.get('[data-testid="billing-billable-items-filter-description"]').setValue('support')

    expect(wrapper.findAll('[data-testid="billing-billable-items-table"] tbody tr')).toHaveLength(1)
    expect(wrapper.get('[data-testid="billing-billable-items-count"]').text()).toContain('1 Records Found')
  })

  it('deletes the ticked items only after the confirmation', async () => {
    const mock = stubRoutes({
      'GET /api/billing/billable-items': itemsPage,
      'GET /api/clients': noClients,
      'DELETE /api/billing/billable-items/5': () => new Response(null, { status: 204 }),
    })
    const wrapper = await mountList()

    await wrapper.get('[data-testid="billing-billable-item-select-5"]').trigger('click')
    await wrapper.get('[data-testid="billing-billable-items-delete"]').trigger('click')
    expect(mock.mock.calls.some(([, init]) => (init as RequestInit | undefined)?.method === 'DELETE')).toBe(false)
    await wrapper.get('[data-testid="dialog-confirm"]').trigger('click')
    await flushPromises()

    expect(mock.mock.calls.some(([, init]) => (init as RequestInit | undefined)?.method === 'DELETE')).toBe(true)
  })

  it('shows the API sentence when the load is refused', async () => {
    stubRoutes({ 'GET /api/billing/billable-items': () => problem('Items are unavailable.', 403), 'GET /api/clients': noClients })
    const wrapper = await mountList()

    expect(wrapper.get('[data-testid="billing-billable-items-error"]').text()).toContain('Items are unavailable.')
  })
})

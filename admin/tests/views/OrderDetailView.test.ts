/**
 * Mount tests for OrderDetailView: the order, its invoice and items render, the actions follow
 * the status, a refused command shows `store.actionError`, and a confirmed delete returns to the
 * list.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import OrderDetailView from '@/views/OrderDetailView.vue'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'
import { json, problem } from '../helpers/fetch'

/** A pending order with one line and a linked invoice. */
const pendingOrder = {
  id: 8,
  orderNumber: 'ORD-0008',
  clientId: 5,
  clientName: 'Ada Lovelace',
  status: 'Pending',
  paymentMethod: 'PayPal',
  total: 30,
  invoiceId: 41,
  invoiceStatus: 'Unpaid',
  itemCount: 1,
  createdAt: '2026-03-04T10:00:00Z',
  ipAddress: '10.0.0.1',
  notes: 'Call the client',
  items: [
    {
      id: 1,
      productId: 3,
      productName: 'Shared Hosting',
      billingCycle: 'monthly',
      domain: 'example.com',
      hostname: null,
      firstPaymentAmount: 30,
      recurringAmount: 10,
      status: 'Pending',
    },
  ],
}

/**
 * Mounts the detail for order 8 and waits for its load.
 *
 * @returns The wrapper and the router.
 */
const mountDetail = async () => {
  const mounted = await mountRouted(OrderDetailView, '/orders/:id', '/orders/8')
  await flushPromises()
  return mounted
}

describe('OrderDetailView', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('renders the order number, status, invoice link, items and notes', async () => {
    stubRoutes({ 'GET /api/orders/8': () => json(pendingOrder) })
    const { wrapper } = await mountDetail()

    expect(wrapper.get('[data-testid="order-header"]').text()).toContain('ORD-0008')
    expect(wrapper.get('[data-testid="order-status"]').text()).toContain('Pending')
    expect(wrapper.get('[data-testid="order-invoice-link"]').attributes('href')).toBe('/billing/41')
    expect(wrapper.get('[data-testid="order-item-1"]').text()).toContain('Shared Hosting')
    expect(wrapper.get('[data-testid="order-item-1"]').text()).toContain('Monthly')
    expect((wrapper.get('[data-testid="order-notes-input"]').element as HTMLTextAreaElement).value).toBe('Call the client')
  })

  it('hides accept, cancel and delete on an active order', async () => {
    stubRoutes({ 'GET /api/orders/8': () => json({ ...pendingOrder, status: 'Active' }) })
    const { wrapper } = await mountDetail()

    expect(wrapper.find('[data-testid="order-accept"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="order-cancel"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="order-delete"]').exists()).toBe(false)
  })

  it('shows the API sentence above the order when accept is refused', async () => {
    stubRoutes({
      'POST /api/orders/8/accept': () => problem('The invoice is not paid.', 409),
      'GET /api/orders/8': () => json(pendingOrder),
    })
    const { wrapper } = await mountDetail()

    await wrapper.get('[data-testid="order-accept"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="order-action-error"]').text()).toContain('The invoice is not paid.')
    expect(wrapper.find('[data-testid="order-header"]').exists()).toBe(true)
  })

  it('asks before deleting, then deletes and returns to the list', async () => {
    const mock = stubRoutes({
      'DELETE /api/orders/8': () => json({}),
      'GET /api/orders/8': () => json(pendingOrder),
      'GET /api/orders?': () => json({ items: [], totalCount: 0 }),
    })
    const { wrapper, router } = await mountDetail()

    await wrapper.get('[data-testid="order-delete"]').trigger('click')
    expect(mock.mock.calls.some(([, init]) => (init as RequestInit | undefined)?.method === 'DELETE')).toBe(false)

    await wrapper.get('[data-testid="dialog-confirm"]').trigger('click')
    await flushPromises()

    expect(router.currentRoute.value.path).toBe('/orders')
  })

  it('stays on the order and shows the sentence when delete is refused', async () => {
    stubRoutes({
      'DELETE /api/orders/8': () => problem('The order has an invoice.', 409),
      'GET /api/orders/8': () => json(pendingOrder),
    })
    const { wrapper, router } = await mountDetail()

    await wrapper.get('[data-testid="order-delete"]').trigger('click')
    await wrapper.get('[data-testid="dialog-confirm"]').trigger('click')
    await flushPromises()

    expect(router.currentRoute.value.path).toBe('/orders/8')
    expect(wrapper.get('[data-testid="order-action-error"]').text()).toContain('The order has an invoice.')
  })

  it('shows the API sentence when the order cannot be loaded', async () => {
    stubRoutes({ 'GET /api/orders/8': () => problem('No such order.', 404) })
    const { wrapper } = await mountDetail()

    expect(wrapper.get('[data-testid="order-error"]').text()).toContain('No such order.')
  })
})

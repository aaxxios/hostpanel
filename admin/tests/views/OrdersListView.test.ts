/**
 * Mount tests for OrdersListView: the table renders what the API sent, a pending order can be
 * accepted (and the list reloads), and a refused load or command shows the API's sentence.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import OrdersListView from '@/views/OrdersListView.vue'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'
import { json, problem } from '../helpers/fetch'

/** One pending order as the list endpoint returns it. */
const pendingOrder = {
  id: 1,
  orderNumber: 'ORD-0001',
  clientId: 5,
  clientName: 'Ada Lovelace',
  status: 'Pending',
  paymentMethod: 'PayPal',
  total: 12.5,
  invoiceId: null,
  itemCount: 2,
  createdAt: '2026-03-04T10:00:00Z',
}

/** One active order, which offers no accept or cancel. */
const activeOrder = { ...pendingOrder, id: 2, orderNumber: 'ORD-0002', status: 'Active' }

/**
 * Mounts the list and waits for its first load.
 *
 * @returns The wrapper.
 */
const mountList = async () => {
  const { wrapper } = await mountRouted(OrdersListView, '/orders', '/orders')
  await flushPromises()
  return wrapper
}

describe('OrdersListView', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('renders a row per order with its status label and total', async () => {
    stubRoutes({ 'GET /api/orders': () => json({ items: [pendingOrder, activeOrder], totalCount: 2 }) })
    const wrapper = await mountList()

    expect(wrapper.get('[data-testid="orders-row-1"]').text()).toContain('ORD-0001')
    expect(wrapper.get('[data-testid="orders-row-1"]').text()).toContain('Ada Lovelace')
    expect(wrapper.get('[data-testid="orders-row-1"]').text()).toContain('$12.50')
    expect(wrapper.get('[data-testid="orders-view-1"]').attributes('href')).toBe('/orders/1')
  })

  it('offers accept and cancel only on a pending order', async () => {
    stubRoutes({ 'GET /api/orders': () => json({ items: [pendingOrder, activeOrder], totalCount: 2 }) })
    const wrapper = await mountList()

    expect(wrapper.find('[data-testid="orders-accept-1"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="orders-cancel-1"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="orders-accept-2"]').exists()).toBe(false)
  })

  it('accepts a pending order and reloads the list', async () => {
    const mock = stubRoutes({
      'POST /api/orders/1/accept': () => json({}),
      'GET /api/orders/1': () => json({ ...pendingOrder, status: 'Active', items: [] }),
      'GET /api/orders?': () => json({ items: [pendingOrder], totalCount: 1 }),
    })
    const wrapper = await mountList()

    await wrapper.get('[data-testid="orders-accept-1"]').trigger('click')
    await flushPromises()

    const requests = mock.mock.calls.map(([url, init]) => `${(init as RequestInit | undefined)?.method ?? 'GET'} ${url as string}`)
    expect(requests).toContain('POST /api/orders/1/accept')
    expect(requests.filter((line) => line.startsWith('GET /api/orders?')).length).toBe(2)
  })

  it('shows the API sentence when a command is refused', async () => {
    stubRoutes({
      'POST /api/orders/1/accept': () => problem('Order cannot be accepted.', 409),
      'GET /api/orders?': () => json({ items: [pendingOrder], totalCount: 1 }),
    })
    const wrapper = await mountList()

    await wrapper.get('[data-testid="orders-accept-1"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="orders-action-error"]').text()).toContain('Order cannot be accepted.')
  })

  it('shows the API sentence when the load is refused', async () => {
    stubRoutes({ 'GET /api/orders': () => problem('Not allowed.', 403) })
    const wrapper = await mountList()

    expect(wrapper.get('[data-testid="orders-error"]').text()).toContain('Not allowed.')
    expect(wrapper.find('[data-testid="orders-table"]').exists()).toBe(false)
  })

  it('shows the empty state when there are no orders', async () => {
    stubRoutes({ 'GET /api/orders': () => json({ items: [], totalCount: 0 }) })
    const wrapper = await mountList()

    expect(wrapper.get('[data-testid="orders-empty"]').text()).toContain('No orders found')
  })
})

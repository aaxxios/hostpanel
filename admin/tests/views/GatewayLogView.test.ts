/**
 * Tests for GatewayLogView: it loads the log from the API, shows the empty state when there is
 * none, and the filters are sent with the next request.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import GatewayLogView from '@/views/GatewayLogView.vue'
import { mountRouted } from '../helpers/mount'
import { callOf, json, stubFetch, problem } from '../helpers/fetch'

/** A log page with one settled payment. */
const page = {
  items: [
    {
      id: 'tx-1',
      dateTime: '2026-10-14T09:00:00Z',
      gateway: 'bank-gateway',
      debugData: 'Invoice #5 ref abc',
      result: 'Payment',
      invoiceId: 5,
    },
  ],
  pageNumber: 1,
  pageSize: 20,
  totalCount: 1,
  totalPages: 1,
}

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('GatewayLogView', () => {
  it('shows the empty state when the log has no entries', async () => {
    setActivePinia(createPinia())
    stubFetch(json({ items: [], pageNumber: 1, pageSize: 20, totalCount: 0, totalPages: 0 }))
    const { wrapper } = await mountRouted(GatewayLogView, '/billing/gateway-log', '/billing/gateway-log')
    await flushPromises()

    expect(wrapper.text()).toContain('Gateway Transaction Log')
    expect(wrapper.get('[data-testid="billing-gateway-log-table"]').text()).toContain('No Records Found')
    expect(wrapper.get('[data-testid="billing-gateway-log-count"]').text()).toContain('0 Records Found')
  })

  it('renders the entries the API returns', async () => {
    setActivePinia(createPinia())
    const mock = stubFetch(json(page))
    const { wrapper } = await mountRouted(GatewayLogView, '/billing/gateway-log', '/billing/gateway-log')
    await flushPromises()

    expect(callOf(mock).url).toBe('/api/gateway-logs?page=1&pageSize=20')
    expect(wrapper.get('[data-testid="billing-gateway-log-table"]').text()).toContain('Invoice #5 ref abc')
    expect(wrapper.get('[data-testid="billing-gateway-log-count"]').text()).toContain('1 Records Found')
  })

  it('sends the debug filter and clears it again', async () => {
    setActivePinia(createPinia())
    const mock = stubFetch(json(page), json(page), json(page))
    const { wrapper } = await mountRouted(GatewayLogView, '/billing/gateway-log', '/billing/gateway-log')
    await flushPromises()

    await wrapper.get('[data-testid="billing-gateway-log-filter-toggle"]').trigger('click')
    await wrapper.get('[data-testid="billing-gateway-log-debug"]').setValue('error')
    await wrapper.get('[data-testid="billing-gateway-log-search"]').trigger('click')
    await flushPromises()
    expect(callOf(mock, 1).url).toBe('/api/gateway-logs?page=1&pageSize=20&debugData=error')

    await wrapper.get('[data-testid="billing-gateway-log-clear"]').trigger('click')
    await flushPromises()
    expect((wrapper.get('[data-testid="billing-gateway-log-debug"]').element as HTMLInputElement).value).toBe('')
    expect(callOf(mock, 2).url).toBe('/api/gateway-logs?page=1&pageSize=20')
  })

  it('loads again from the Retry of a refused load', async () => {
    stubFetch(problem('Not allowed.', 403))
    const wrapper = (await mountRouted(GatewayLogView, '/billing/gateway-log', '/billing/gateway-log')).wrapper
    await flushPromises()
    const fetchMock = vi.mocked(fetch)
    const before = fetchMock.mock.calls.length

    await wrapper.get('[data-testid="billing-gateway-log-error-retry"]').trigger('click')
    await flushPromises()

    expect(fetchMock.mock.calls.length).toBeGreaterThan(before)
  })
})

/**
 * Mount tests of CancellationRequestsView (cancellation requests): it renders what the API sent, shows the empty state where it
 * has one, and shows the API's sentence when the load is refused.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import CancellationRequestsView from '@/views/CancellationRequestsView.vue'
import { mountRoutedSettled } from '../helpers/mount'
import { json, problem } from '../helpers/fetch'
import { stubRoutes } from '../helpers/routes'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('CancellationRequestsView', () => {
  /** One open, immediate cancellation as the list endpoint returns it. */
  const request = {
    id: 5,
    serviceId: 2,
    serviceName: 'Shared Hosting',
    clientId: 9,
    clientName: 'Grace Hopper',
    type: 'Immediate',
    reason: 'Moving away',
    status: 'Open',
    createdAt: '2026-05-01T09:00:00Z',
  }

  it('renders the request with its type and status words', async () => {
    stubRoutes({ 'GET /api/admin/cancellation-requests': () => json({ items: [request], totalCount: 1 }) })
    const wrapper = await mountRoutedSettled(CancellationRequestsView, '/clients/cancellations')

    const row = wrapper.get('[data-testid="cancellations-row-5"]').text()
    expect(row).toContain('Shared Hosting')
    expect(row).toContain('Immediate')
    expect(row).toContain('Open')
    expect(wrapper.get('[data-testid="cancellations-client-5"]').attributes('href')).toBe('/clients/9')
  })

  it('asks before deleting, then deletes and reloads', async () => {
    const mock = stubRoutes({
      'DELETE /api/admin/cancellation-requests/5': () => new Response(null, { status: 204 }),
      'GET /api/admin/cancellation-requests': () => json({ items: [request], totalCount: 1 }),
    })
    const wrapper = await mountRoutedSettled(CancellationRequestsView, '/clients/cancellations')

    await wrapper.get('[data-testid="cancellations-delete-5"]').trigger('click')
    expect(wrapper.find('[data-testid="cancellations-delete-dialog"]').exists()).toBe(true)
    expect(mock.mock.calls.some(([, init]) => (init as RequestInit | undefined)?.method === 'DELETE')).toBe(false)

    await wrapper.get('[data-testid="cancellations-delete-dialog"] [data-testid="dialog-confirm"]').trigger('click')
    await flushPromises()

    expect(mock.mock.calls.some(([, init]) => (init as RequestInit | undefined)?.method === 'DELETE')).toBe(true)
  })

  it('shows the empty state and the refusal sentence', async () => {
    stubRoutes({ 'GET /api/admin/cancellation-requests': () => json({ items: [], totalCount: 0 }) })
    const empty = await mountRoutedSettled(CancellationRequestsView, '/clients/cancellations')
    expect(empty.get('[data-testid="cancellations-empty"]').text()).toContain('No cancellation requests found')

    stubRoutes({ 'GET /api/admin/cancellation-requests': () => problem('Not allowed.', 403) })
    const refused = await mountRoutedSettled(CancellationRequestsView, '/clients/cancellations')
    expect(refused.get('[data-testid="cancellations-error"]').text()).toContain('Not allowed.')
  })
})

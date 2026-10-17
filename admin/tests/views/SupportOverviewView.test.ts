/**
 * Smoke tests for SupportOverviewView: the period request, the stat tiles, the no-data states and
 * the refusal sentence, through the real store and transport with `fetch` stubbed.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import SupportOverviewView from '@/views/SupportOverviewView.vue'
import { mountView } from '../helpers/mount'
import { problem, stubFetch, callOf } from '../helpers/fetch'
import { stubPathRoutes } from '../helpers/routes'

/** Overview figures with no tickets in any hour. */
const stats = {
  newTickets: 4,
  clientReplies: 7,
  staffReplies: 9,
  ticketsWithoutReply: 2,
  averageFirstResponse: '1h 5m',
  ticketsByHour: Array.from({ length: 24 }, () => 0),
}

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('SupportOverviewView', () => {
  it('asks for today and shows the figures', async () => {
    const mock = stubPathRoutes([{ path: '/tickets/overview', body: stats }])

    const wrapper = await mountView(SupportOverviewView)

    expect(callOf(mock).url).toBe('/api/tickets/overview?period=today')
    expect(wrapper.get('[data-testid="support-overview-stat-new-tickets"]').text()).toContain('4')
    expect(wrapper.get('[data-testid="support-overview-stat-staff-replies"]').text()).toContain('9')
    expect(wrapper.get('[data-testid="support-overview-average"]').text()).toBe('1h 5m')
  })

  it('shows the no-data message when no hour has tickets', async () => {
    stubPathRoutes([{ path: '/tickets/overview', body: stats }])

    const wrapper = await mountView(SupportOverviewView)

    expect(wrapper.get('[data-testid="support-overview-hours-empty"]').text()).toBe('No data')
  })

  it('shows the API sentence when the overview is refused', async () => {
    stubFetch(problem('Not allowed.', 403))

    const wrapper = await mountView(SupportOverviewView)

    expect(wrapper.get('[data-testid="support-overview-error"]').text()).toBe('Not allowed.')
  })
})

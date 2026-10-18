/**
 * Smoke tests for SupportTicketDetailView: the loaded ticket and thread, the reply form and the
 * close action, through the real store and transport with `fetch` stubbed.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import SupportTicketDetailView from '@/views/SupportTicketDetailView.vue'
import { TicketPriority, TicketStatus } from '@/types/ticket'
import { mountView } from '../helpers/mount'
import { problem, stubFetch, callOf } from '../helpers/fetch'
import { stubPathRoutes } from '../helpers/routes'

/** A ticket with one staff reply. */
const ticket = {
  id: 5,
  clientId: 3,
  subject: 'DNS is down',
  message: 'Nothing resolves.',
  status: TicketStatus.Open,
  priority: TicketPriority.Medium,
  departmentId: 1,
  departmentName: 'Support',
  assignedToStaffId: null,
  assignedToStaffName: null,
  createdAt: '2026-10-01T00:00:00Z',
  replies: [{ id: 1, message: 'Looking into it.', authorName: 'Ann', isStaffReply: true, createdAt: '2026-10-01T01:00:00Z' }],
}

/** Where the view sits in the router. */
const location = { path: '/support/tickets/5', pattern: '/support/tickets/:id' }

/** The two endpoints the page loads on mount. */
const loadRoutes = [
  { path: '/tickets/5', body: ticket },
  { path: '/tickets/departments', body: [{ id: 1, name: 'Support', email: 'support@example.test' }] },
]

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('SupportTicketDetailView', () => {
  it('shows the ticket, its first message and the thread', async () => {
    stubPathRoutes(loadRoutes)

    const wrapper = await mountView(SupportTicketDetailView, location)

    expect(wrapper.get('[data-testid="ticket-title"]').text()).toContain('DNS is down')
    expect(wrapper.get('[data-testid="ticket-initial-message"]').text()).toContain('Nothing resolves.')
    expect(wrapper.findAll('[data-testid="ticket-reply"]')).toHaveLength(1)
    expect(wrapper.get('[data-testid="ticket-reply"]').text()).toContain('Looking into it.')
  })

  it('keeps the reply button disabled until the message is filled, then sends the reply without an author', async () => {
    const mock = stubPathRoutes([...loadRoutes, { path: '/tickets/5/reply', body: null }])
    const wrapper = await mountView(SupportTicketDetailView, location)
    const submit = wrapper.get('[data-testid="ticket-reply-submit"]')
    expect(submit.attributes('disabled')).toBeDefined()

    await wrapper.get('[data-testid="ticket-reply-message"]').setValue('Fixed now.')
    expect(submit.attributes('disabled')).toBeUndefined()
    await submit.trigger('click')
    await flushPromises()

    const reply = mock.mock.calls.map((_, index) => callOf(mock, index)).find((call) => call.method === 'POST')
    expect(reply).toMatchObject({ url: '/api/tickets/5/reply', body: { message: 'Fixed now.' } })
  })

  it('closes the ticket with a status update', async () => {
    const mock = stubPathRoutes(loadRoutes)
    const wrapper = await mountView(SupportTicketDetailView, location)

    await wrapper.get('[data-testid="ticket-close"]').trigger('click')
    await flushPromises()

    const update = mock.mock.calls.map((_, index) => callOf(mock, index)).find((call) => call.method === 'PUT')
    expect(update).toMatchObject({ url: '/api/tickets/5', body: { status: 'Closed' } })
  })

  it('shows the API sentence when the ticket is refused', async () => {
    stubFetch(problem('Ticket not found.', 404))

    const wrapper = await mountView(SupportTicketDetailView, location)

    expect(wrapper.get('[data-testid="ticket-error"]').text()).toBe('Ticket not found.')
  })
})

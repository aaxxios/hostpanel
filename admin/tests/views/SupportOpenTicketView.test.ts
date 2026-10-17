/**
 * Smoke tests for SupportOpenTicketView: the form renders with placeholders, loads departments and
 * cannot be submitted without a client, through the real stores and transport with `fetch` stubbed.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import SupportOpenTicketView from '@/views/SupportOpenTicketView.vue'
import { callOf } from '../helpers/fetch'
import { mountView } from '../helpers/mount'
import { stubPathRoutes } from '../helpers/routes'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('SupportOpenTicketView', () => {
  it('loads the departments and renders the form with its placeholders', async () => {
    const mock = stubPathRoutes([{ path: '/tickets/departments', body: [{ id: 1, name: 'Support', email: 'support@example.test' }] }])

    const wrapper = await mountView(SupportOpenTicketView)

    expect(callOf(mock).url).toBe('/api/tickets/departments')
    expect(wrapper.get('[data-testid="support-open-ticket-subject"]').attributes('placeholder')).toBe('Ticket subject')
    expect(wrapper.get('[data-testid="support-open-ticket-message"]').attributes('placeholder')).toBeTruthy()
    expect(wrapper.get('[data-testid="support-open-ticket-client"]').attributes('placeholder')).toBeTruthy()
  })

  it('cannot be submitted before a client is chosen, even with subject and message', async () => {
    stubPathRoutes([{ path: '/tickets/departments', body: [] }])
    const wrapper = await mountView(SupportOpenTicketView)

    await wrapper.get('[data-testid="support-open-ticket-subject"]').setValue('Help')
    await wrapper.get('[data-testid="support-open-ticket-message"]').setValue('Please help.')

    expect(wrapper.get('[data-testid="support-open-ticket-submit"]').attributes('disabled')).toBeDefined()
  })
})

/**
 * Mount tests for ClientEmailsView: it renders against the real kit, settles on an API refusal and captions each send outcome.
 */
import { describe, expect, it, vi } from 'vitest'
import ClientEmailsView from '@/views/ClientEmailsView.vue'
import { mountClientView } from '../helpers/clientViews'
import { json, stubFetch } from '../helpers/fetch'
import { flushPromises } from '@vue/test-utils'
import { EmailSendFailure } from '@/types/emaillog'
import type { EmailLog } from '@/types/emaillog'
import type { PaginatedList } from '@/types/paginatedlist'

describe('ClientEmailsView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientEmailsView)

    expect(wrapper.exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientEmailsView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })

  it('loads again from the Retry of a refused load', async () => {
    const wrapper = await mountClientView(ClientEmailsView)
    await flushPromises()
    const fetchMock = vi.mocked(fetch)
    const before = fetchMock.mock.calls.length

    await wrapper.get('[data-testid="client-emails-error-retry"]').trigger('click')
    await flushPromises()

    expect(fetchMock.mock.calls.length).toBeGreaterThan(before)
  })

  it('captions each entry as delivered or with its translated failure reason', async () => {
    const wrapper = await mountClientView(ClientEmailsView)
    const page: PaginatedList<EmailLog> = {
      items: [
        { id: 1, to: 'a@example.test', subject: 'Welcome', sentAt: '2026-10-01T08:00:00Z', success: true, failure: null },
        { id: 2, to: 'a@example.test', subject: 'Invoice', sentAt: '2026-10-02T08:00:00Z', success: false, failure: EmailSendFailure.RecipientRejected },
      ],
      pageNumber: 1,
      pageSize: 25,
      totalCount: 2,
      totalPages: 1,
    }
    stubFetch(json(page))

    await wrapper.get('[data-testid="client-emails-error-retry"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="client-emails-outcome-1"]').text()).toBe('Delivered')
    expect(wrapper.get('[data-testid="client-emails-outcome-2"]').text()).toBe('Recipient address rejected')
  })
})

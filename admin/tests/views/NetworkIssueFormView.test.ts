/**
 * Smoke tests for NetworkIssueFormView: create posts the form with enum defaults, edit loads the
 * issue and shows the status field, through the real store and transport with `fetch` stubbed.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import NetworkIssueFormView from '@/views/NetworkIssueFormView.vue'
import { NetworkIssuePriority, NetworkIssueStatus, NetworkIssueType } from '@/types/networkissue'
import { callOf } from '../helpers/fetch'
import { mountView } from '../helpers/mount'
import { stubPathRoutes } from '../helpers/routes'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('NetworkIssueFormView', () => {
  it('hides the status field on create and posts the form with its defaults', async () => {
    const mock = stubPathRoutes([{ path: '/network-issues', body: 5 }])
    const wrapper = await mountView(NetworkIssueFormView)
    expect(wrapper.find('[data-testid="network-issue-form-status"]').exists()).toBe(false)
    expect(wrapper.get('[data-testid="network-issue-form-save"]').attributes('disabled')).toBeDefined()

    await wrapper.get('[data-testid="network-issue-form-title"]').setValue('Packet loss')
    await wrapper.get('[data-testid="network-issue-form-save"]').trigger('click')
    await flushPromises()

    expect(callOf(mock)).toMatchObject({
      url: '/api/network-issues',
      method: 'POST',
      body: {
        title: 'Packet loss',
        type: NetworkIssueType.Server,
        server: null,
        priority: NetworkIssuePriority.Medium,
        status: NetworkIssueStatus.Reported,
        endDate: null,
      },
    })
  })

  it('loads the issue in edit mode, shows the status field and updates it', async () => {
    const mock = stubPathRoutes([
      {
        path: '/network-issues/2',
        body: {
          id: 2,
          title: 'Packet loss',
          type: NetworkIssueType.Other,
          server: null,
          priority: NetworkIssuePriority.High,
          status: NetworkIssueStatus.Scheduled,
          startDate: '2026-10-01T08:00:00Z',
          endDate: null,
          description: 'Maintenance',
          createdAt: '2026-10-01T08:00:00Z',
        },
      },
    ])
    const wrapper = await mountView(NetworkIssueFormView, {
      path: '/support/network-issues/2/edit',
      pattern: '/support/network-issues/:id/edit',
    })

    expect(wrapper.find('[data-testid="network-issue-form-status"]').exists()).toBe(true)
    expect((wrapper.get('[data-testid="network-issue-form-title"]').element as HTMLInputElement).value).toBe('Packet loss')

    await wrapper.get('[data-testid="network-issue-form-save"]').trigger('click')
    await flushPromises()

    expect(callOf(mock, 1)).toMatchObject({
      url: '/api/network-issues/2',
      method: 'PUT',
      body: { type: NetworkIssueType.Other, priority: NetworkIssuePriority.High, status: NetworkIssueStatus.Scheduled },
    })
  })
})

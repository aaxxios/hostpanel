/**
 * Smoke tests for AnnouncementsListView: the list, the delete confirmation and the empty and error
 * states, through the real store and transport with `fetch` stubbed.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import AnnouncementsListView from '@/views/AnnouncementsListView.vue'
import { mountView } from '../helpers/mount'
import { problem, stubFetch, callOf } from '../helpers/fetch'
import { stubPathRoutes } from '../helpers/routes'

/** One announcement as the list endpoint returns it. */
const announcement = { id: 3, title: 'Maintenance window', content: 'Tonight', isPublished: true, createdAt: '2026-10-01T10:30:00Z' }

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('AnnouncementsListView', () => {
  it('lists the announcements with their published state', async () => {
    stubPathRoutes([{ path: '/announcements', body: { items: [announcement], totalCount: 1 } }])

    const wrapper = await mountView(AnnouncementsListView)
    const row = wrapper.get('[data-testid="announcements-row-3"]')

    expect(row.text()).toContain('Maintenance window')
    expect(row.text()).toContain('Yes')
    expect(wrapper.get('[data-testid="announcements-count"]').text()).toContain('1')
  })

  it('asks for confirmation before deleting, then deletes and reloads', async () => {
    const mock = stubPathRoutes([{ path: '/announcements', body: { items: [announcement], totalCount: 1 } }])
    const wrapper = await mountView(AnnouncementsListView)
    expect(wrapper.find('[role="dialog"]').exists()).toBe(false)

    await wrapper.get('[data-testid="announcements-3-delete"]').trigger('click')
    expect(wrapper.find('[role="dialog"]').exists()).toBe(true)
    await wrapper.get('[data-testid="dialog-confirm"]').trigger('click')
    await flushPromises()

    expect(callOf(mock, 1)).toMatchObject({ url: '/api/announcements/3', method: 'DELETE' })
    expect(wrapper.find('[role="dialog"]').exists()).toBe(false)
  })

  it('shows the empty state when there are no announcements', async () => {
    stubPathRoutes([{ path: '/announcements', body: { items: [], totalCount: 0 } }])

    const wrapper = await mountView(AnnouncementsListView)

    expect(wrapper.get('[data-testid="announcements-table"]').text()).toContain('No announcements found')
  })

  it('shows the API sentence when the list is refused', async () => {
    stubFetch(problem('Not allowed.', 403))

    const wrapper = await mountView(AnnouncementsListView)

    expect(wrapper.get('[data-testid="announcements-error"]').text()).toBe('Not allowed.')
  })
})

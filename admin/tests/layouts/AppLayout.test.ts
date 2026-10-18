/**
 * Mount tests for AppLayout: the shell holds the sidebar, the topbar and the routed content, and
 * the hamburger opens the navigation drawer.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import AppLayout from '@/layouts/AppLayout.vue'
import { stubRoutes } from '../helpers/routes'
import { json } from '../helpers/fetch'
import { mountView } from '../helpers/mount'

describe('AppLayout', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('renders the desktop sidebar, the topbar and the content area', async () => {
    stubRoutes({ 'GET /api/admin/dashboard/activity': () => json([]) })
    const wrapper = await mountView(AppLayout, { path: '/dashboard' })

    expect(wrapper.find('[data-testid="app-layout"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="app-sidebar-desktop"] [data-testid="app-sidebar"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="app-topbar"]').exists()).toBe(true)
  })

  it('opens the navigation drawer from the hamburger', async () => {
    stubRoutes({ 'GET /api/admin/dashboard/activity': () => json([]) })
    const wrapper = await mountView(AppLayout, { path: '/dashboard' })
    expect(wrapper.findAll('[data-testid="app-sidebar"]')).toHaveLength(1)

    await wrapper.get('[data-testid="topbar-toggle-sidebar"]').trigger('click')
    await flushPromises()

    expect(wrapper.findAll('[data-testid="app-sidebar"]')).toHaveLength(2)
  })
})

/**
 * Mount tests for AppLayout: the shell holds the sidebar, the topbar and the routed content, the
 * hamburger opens the navigation drawer, and a screen still downloading is answered in the content
 * area while the shell stays.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import type { RouteLocationNormalized } from 'vue-router'
import AppLayout from '@/layouts/AppLayout.vue'
import { useNavigationStore } from '@/stores/navigation'
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

  it('shows the skeleton in the content area while the next screen downloads, and keeps the shell', async () => {
    stubRoutes({ 'GET /api/admin/dashboard/activity': () => json([]) })
    const wrapper = await mountView(AppLayout, { path: '/dashboard' })
    expect(wrapper.find('[data-testid="view-loading"]').exists()).toBe(false)

    // A navigation the guards admitted, heading for a screen drawn by another record than the current one.
    useNavigationStore().pending = { path: '/clients', matched: [{ components: { default: {} } }] } as unknown as RouteLocationNormalized
    await flushPromises()

    expect(wrapper.find('main [data-testid="view-loading"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="app-sidebar-desktop"] [data-testid="app-sidebar"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="app-topbar"]').exists()).toBe(true)
  })
})

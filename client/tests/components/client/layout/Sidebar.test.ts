/**
 * Unit tests for ClientLayoutSidebar: the links, the active section, the mobile drawer state and
 * the sign-out button.
 *
 * @module tests/components/client/layout/Sidebar.test
 */

import { describe, expect, it, vi } from 'vitest'
import { Globe, Server } from 'lucide-vue-next'
import { mountClient } from '~/tests/helpers/mountClient'
import Sidebar from '~/components/client/layout/Sidebar.vue'

/** Two links, as the layout would pass after filtering by permission. */
const ITEMS = [
  { to: '/client/services', label: 'My Services', icon: Server },
  { to: '/client/domains', label: 'My Domains', icon: Globe }
]

/**
 * Stands in for the route the sidebar highlights from.
 *
 * @param path - Current route path.
 */
const routeAt = (path: string): void => {
  vi.stubGlobal('useRoute', () => ({ params: {}, query: {}, path, fullPath: path, meta: {} }))
}

describe('ClientLayoutSidebar', () => {
  it('renders one link per item', () => {
    routeAt('/')
    const wrapper = mountClient(Sidebar, { props: { items: ITEMS, open: false } })

    expect(wrapper.find('[data-testid="nav-client-services"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="nav-client-domains"]').exists()).toBe(true)
  })

  it('highlights the section the route is inside, including nested pages', () => {
    routeAt('/client/services/16/setup')
    const wrapper = mountClient(Sidebar, { props: { items: ITEMS, open: false } })

    expect(wrapper.find('[data-testid="nav-client-services"]').classes()).toContain('bg-primary-500/10')
    expect(wrapper.find('[data-testid="nav-client-domains"]').classes()).not.toContain('bg-primary-500/10')
  })

  it('does not treat a longer sibling path as the same section', () => {
    routeAt('/client/services-archive')
    const wrapper = mountClient(Sidebar, { props: { items: ITEMS, open: false } })

    expect(wrapper.find('[data-testid="nav-client-services"]').classes()).not.toContain('bg-primary-500/10')
  })

  it('slides in when open and out when closed', () => {
    routeAt('/')
    const open = mountClient(Sidebar, { props: { items: ITEMS, open: true } })
    const closed = mountClient(Sidebar, { props: { items: ITEMS, open: false } })

    expect(open.find('[data-testid="client-sidebar"]').classes()).toContain('translate-x-0')
    expect(closed.find('[data-testid="client-sidebar"]').classes()).toContain('-translate-x-full')
  })

  it('emits close when a link is followed and when the close button is pressed', async () => {
    routeAt('/')
    const wrapper = mountClient(Sidebar, { props: { items: ITEMS, open: true } })

    await wrapper.find('[data-testid="nav-client-services"]').trigger('click')
    await wrapper.find('[data-testid="client-sidebar-close"]').trigger('click')

    expect(wrapper.emitted('close')).toHaveLength(2)
  })

  it('emits signOut from the bottom button', async () => {
    routeAt('/')
    const wrapper = mountClient(Sidebar, { props: { items: ITEMS, open: false } })

    await wrapper.find('[data-testid="client-sign-out"]').trigger('click')

    expect(wrapper.emitted('signOut')).toHaveLength(1)
  })
})

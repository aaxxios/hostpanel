/**
 * Mount tests for ClientInnerSidebar: entries and the active mark.
 */
import { describe, expect, it } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createMemoryHistory, createRouter } from 'vue-router'
import ClientInnerSidebar from '@/components/clients/ClientInnerSidebar.vue'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Mounts the sidebar on the given path.
 *
 * @param path - Route the router starts at.
 * @returns The wrapper.
 */
const mountSidebar = async (path: string) => {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [{ path: '/:pathMatch(.*)*', component: { render: () => null } }],
  })
  await router.push(path)
  await router.isReady()
  const wrapper = mount(ClientInnerSidebar, {
    props: { clientId: '12' },
    global: { plugins: [createTestI18n(), router] },
  })
  await flushPromises()
  return wrapper
}

describe('ClientInnerSidebar', () => {
  it('links every section under the client path', async () => {
    const wrapper = await mountSidebar('/clients/12/summary')

    expect(wrapper.findAll('a')).toHaveLength(13)
    expect(wrapper.get('[data-testid="client-nav-invoices"]').attributes('href')).toBe('/clients/12/invoices')
    expect(wrapper.get('[data-testid="client-nav-billable"]').attributes('href')).toBe('/clients/12/billable-items')
  })

  it('marks the entry of the current route', async () => {
    const wrapper = await mountSidebar('/clients/12/tickets/4')

    expect(wrapper.get('[data-testid="client-nav-tickets"]').attributes('aria-current')).toBe('page')
    expect(wrapper.get('[data-testid="client-nav-summary"]').attributes('aria-current')).toBeUndefined()
  })
})

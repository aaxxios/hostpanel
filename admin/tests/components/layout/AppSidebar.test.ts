/**
 * Mount tests for AppSidebar: the top-level entries, sub-menus that open for the current section
 * or on click, the active marker, the navigate event and the sign-out button.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import AppSidebar from '@/components/layout/AppSidebar.vue'
import { stubRoutes } from '../../helpers/routes'
import { mountView } from '../../helpers/mount'

/**
 * Mounts the sidebar at a path.
 *
 * @param path - The current route.
 * @returns The wrapper.
 */
const mountSidebar = async (path: string) => {
  stubRoutes({})
  return mountView(AppSidebar, { path })
}

describe('AppSidebar', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('renders every top-level entry as a link or a section toggle', async () => {
    const wrapper = await mountSidebar('/dashboard')

    expect(wrapper.get('[data-testid="nav-dashboard"]').attributes('href')).toBe('/dashboard')
    expect(wrapper.get('[data-testid="nav-servers"]').attributes('href')).toBe('/servers')
    expect(wrapper.get('[data-testid="nav-migration"]').attributes('href')).toBe('/migration')
    expect(wrapper.get('[data-testid="nav-clients"]').element.tagName).toBe('BUTTON')
  })

  it('opens the sub-menu of the current section and marks the current child', async () => {
    const wrapper = await mountSidebar('/clients/users')

    expect(wrapper.get('[data-testid="nav-clients"]').attributes('aria-expanded')).toBe('true')
    expect(wrapper.get('[data-testid="nav-clients-users"]').attributes('href')).toBe('/clients/users')
    expect(wrapper.get('[data-testid="nav-clients-users"]').classes()).toContain('text-primary-400')
  })

  it('lists the five storefront content pages and opens the section on one of them', async () => {
    const wrapper = await mountSidebar('/storefront/faqs')

    expect(wrapper.get('[data-testid="nav-storefront"]').attributes('aria-expanded')).toBe('true')
    expect(wrapper.get('[data-testid="nav-storefront-testimonials"]').attributes('href')).toBe('/storefront/testimonials')
    expect(wrapper.get('[data-testid="nav-storefront-partners"]').attributes('href')).toBe('/storefront/partners')
    expect(wrapper.get('[data-testid="nav-storefront-projects"]').attributes('href')).toBe('/storefront/projects')
    expect(wrapper.get('[data-testid="nav-storefront-faqs"]').classes()).toContain('text-primary-400')
    expect(wrapper.get('[data-testid="nav-storefront-locations"]').attributes('href')).toBe('/storefront/locations')
  })

  it('opens a closed sub-menu when its toggle is clicked', async () => {
    const wrapper = await mountSidebar('/dashboard')
    expect(wrapper.get('[data-testid="nav-billing"]').attributes('aria-expanded')).toBe('false')

    await wrapper.get('[data-testid="nav-billing"]').trigger('click')

    expect(wrapper.get('[data-testid="nav-billing"]').attributes('aria-expanded')).toBe('true')
  })

  it('emits navigate when a link is clicked', async () => {
    const wrapper = await mountSidebar('/dashboard')

    await wrapper.get('[data-testid="nav-orders"]').trigger('click')
    await flushPromises()

    expect(wrapper.emitted('navigate')).toHaveLength(1)
  })

  it('renders the built-in wordmark for each theme', async () => {
    const wrapper = await mountSidebar('/dashboard')

    expect(wrapper.get('[data-testid="brand-logo-dark"]').attributes('src')).toBe('/logo-dark.svg')
    expect(wrapper.get('[data-testid="brand-logo-light"]').attributes('src')).toBe('/logo.svg')
    expect(wrapper.get('[data-testid="brand-logo-dark"]').attributes('alt')).toBe('Hostpanel')
  })

  it('offers sign-out', async () => {
    const wrapper = await mountSidebar('/dashboard')

    expect(wrapper.find('[data-testid="sidebar-sign-out"]').exists()).toBe(true)
  })
})

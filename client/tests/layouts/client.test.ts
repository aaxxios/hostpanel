/**
 * Unit tests for `layouts/client.vue` against the real auth and client stores: the
 * permission-filtered navigation, the mobile drawer, the identity fetch, the platform widget
 * hand-over and signing out. Only the API composables are replaced.
 *
 * @module tests/layouts/client.test
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import type { VueWrapper } from '@vue/test-utils'
import { nextTick } from 'vue'
import { storeToRefs } from 'pinia'
import { mountClient } from '~/tests/helpers/mountClient'
import { stubAuthRuntime } from '~/tests/helpers/authScreens'

/** The mocked identity call, re-created for every test. */
const fetchMe = vi.fn()

/** The mocked local sign-out call, re-created for every test. */
const logout = vi.fn()

vi.mock('~/composables/apis/useClientApi', () => ({
  /**
   * Stands in for the real API composable so the client store runs without transport.
   *
   * @returns Only the endpoint the layout reaches.
   */
  useClientApi: () => ({ fetchMe })
}))

vi.mock('~/composables/apis/useAuthApi', () => ({
  /**
   * Stands in for the real API composable so the auth store runs without transport.
   *
   * @returns Only the endpoint the layout reaches.
   */
  useAuthApi: () => ({ logout })
}))

const { default: ClientLayout } = await import('~/layouts/client.vue')
const { default: LayoutDemoBanner } = await import('~/components/layout/LayoutDemoBanner.vue')
const { Permission, usePermissions } = await import('~/composables/usePermissions')
const { useAuthStore } = await import('~/stores/auth')

/** The theme and language controls need runtime services this test does not provide. */
const STUBS = { UiThemeToggle: true, UiLanguageSwitcher: true, UiCookieBanner: { template: '<div data-testid="cookie-banner" />' } }

/** The signed-in person the identity call answers with. */
const ME = { firstname: 'Ada', lastname: 'Lovelace', email: 'ada@example.com', permissions: Permission.All }

beforeEach(() => {
  stubAuthRuntime()
  vi.stubGlobal('usePermissions', usePermissions)
  vi.stubGlobal('useAuthStore', useAuthStore)
  vi.stubGlobal('storeToRefs', storeToRefs)
  fetchMe.mockReset()
  fetchMe.mockResolvedValue(ME)
  logout.mockReset()
  logout.mockResolvedValue({})
})

/** Every layout a test mounted; each is unmounted afterwards so its widget poll never outlives the test. */
const mounted: VueWrapper[] = []

afterEach(() => {
  mounted.splice(0).forEach((wrapper) => {
    wrapper.unmount()
  })
})

/**
 * Mounts the layout with a page in its slot and waits for the identity fetch.
 *
 * @returns The wrapper.
 */
const mountLayout = async () => {
  const wrapper = mountClient(ClientLayout, { slots: { default: 'page content' }, stubs: STUBS, components: { LayoutDemoBanner } })
  mounted.push(wrapper)
  await flushPromises()
  return wrapper
}

describe('client layout', () => {
  it('mounts the cookie banner inside the client area', async () => {
    const wrapper = await mountLayout()

    expect(wrapper.find('[data-testid="cookie-banner"]').exists()).toBe(true)
  })

  it('renders the page in its slot', async () => {
    const wrapper = await mountLayout()

    expect(wrapper.text()).toContain('page content')
  })

  it('sets its own text colour for both themes instead of inheriting the body colour', async () => {
    const wrapper = await mountLayout()

    // The body keeps the dark page colour and white text outside the aurora template, so a
    // child that inherits (the sidebar brand) was white on the white light-mode sidebar.
    const classes = wrapper.find('[data-testid="client-layout"]').classes()
    expect(classes).toContain('text-gray-900')
    expect(classes).toContain('dark:text-white')
  })

  it('fetches the identity once and greets the person by name', async () => {
    const wrapper = await mountLayout()

    expect(fetchMe).toHaveBeenCalledTimes(1)
    expect(wrapper.find('[data-testid="client-topbar-name"]').text()).toBe('Ada Lovelace')
  })

  it('hands the person to the platform widget mounts', async () => {
    const wrapper = await mountLayout()

    const user = JSON.parse(wrapper.find('#inno-account-mount').attributes('data-user') ?? '{}')
    expect(user).toEqual({ email: 'ada@example.com', firstName: 'Ada', lastName: 'Lovelace' })
  })

  it('shows every link to a person with all permissions', async () => {
    const wrapper = await mountLayout()
    useAuthStore().user = { ...ME } as never
    await nextTick()

    for (const id of ['dashboard', 'services', 'domains', 'invoices', 'tickets', 'account']) {
      expect(wrapper.find(`[data-testid="nav-client-${id}"]`).exists()).toBe(true)
    }
    expect(wrapper.find('[data-testid="nav-announcements"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="nav-knowledgebase"]').exists()).toBe(true)
  })

  it('hides the sections a person has no permission for', async () => {
    const wrapper = await mountLayout()
    useAuthStore().user = { ...ME, permissions: Permission.ViewDomains } as never
    await nextTick()

    expect(wrapper.find('[data-testid="nav-client-domains"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="nav-client-services"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="nav-client-invoices"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="nav-client-dashboard"]').exists()).toBe(true)
  })

  it('opens the mobile drawer with the menu button and closes it from the backdrop', async () => {
    const wrapper = await mountLayout()
    expect(wrapper.find('[data-testid="client-sidebar-backdrop"]').exists()).toBe(false)

    await wrapper.find('[data-testid="client-menu-toggle"]').trigger('click')
    expect(wrapper.find('[data-testid="client-sidebar"]').classes()).toContain('translate-x-0')
    expect(wrapper.find('[data-testid="client-sidebar-backdrop"]').exists()).toBe(true)

    await wrapper.find('[data-testid="client-sidebar-backdrop"]').trigger('click')
    expect(wrapper.find('[data-testid="client-sidebar"]').classes()).toContain('-translate-x-full')
  })

  it('closes the drawer when a link is followed', async () => {
    const wrapper = await mountLayout()
    await wrapper.find('[data-testid="client-menu-toggle"]').trigger('click')

    await wrapper.find('[data-testid="nav-client-dashboard"]').trigger('click')

    expect(wrapper.find('[data-testid="client-sidebar-backdrop"]').exists()).toBe(false)
  })

  it('signs out through the auth store and returns to the sign-in page', async () => {
    const wrapper = await mountLayout()

    await wrapper.find('[data-testid="client-sign-out"]').trigger('click')
    await flushPromises()

    expect(logout).toHaveBeenCalledTimes(1)
    expect(navigateTo).toHaveBeenCalledWith('/client/login')
  })

  it('re-initialises the platform widget once its script has loaded', async () => {
    const reinit = vi.fn()
    Object.assign(window, { InnoWidget: { reinit } })
    await mountLayout()

    await new Promise((resolve) => setTimeout(resolve, 120))

    expect(reinit).toHaveBeenCalled()
    Reflect.deleteProperty(window, 'InnoWidget')
  })

  it('stops looking for the platform widget once the layout is gone', async () => {
    vi.useFakeTimers()
    try {
      const wrapper = await mountLayout()
      wrapper.unmount()
      mounted.splice(mounted.indexOf(wrapper), 1)
      const reinit = vi.fn()
      Object.assign(window, { InnoWidget: { reinit } })

      vi.runAllTimers()

      expect(reinit).not.toHaveBeenCalled()
    } finally {
      vi.useRealTimers()
      Reflect.deleteProperty(window, 'InnoWidget')
    }
  })
})

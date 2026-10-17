/**
 * Unit tests for LayoutHeader: the desktop navigation, the dropdown group, the mobile menu, the
 * client-area entry in both states, sign-out and the scroll-driven bar style.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { defineComponent, h, nextTick } from 'vue'
import { flushPromises } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import LayoutCartButton from '~/components/layout/LayoutCartButton.vue'
import LayoutHeader from '~/components/layout/LayoutHeader.vue'
import { useAuthStore } from '~/stores/auth'
import type { ClientUser } from '~/types/clientuser'

/** The mocked sign-out call of the auth API. */
const logoutMock = vi.fn()

vi.mock('~/composables/apis/useAuthApi', () => ({
  /**
   * Stands in for the auth API composable so the store runs without transport.
   *
   * @returns Only the endpoint the header reaches for.
   */
  useAuthApi: () => ({ logout: logoutMock, ssoLogout: logoutMock })
}))

/** Stand-in for the logo component, which needs the brand plugin. */
const AppLogoStub = defineComponent({
  name: 'AppLogo',
  setup: () => () => h('img', { 'data-testid': 'app-logo-stub' }),
})

/** Stand-in for the kit's language switcher, which needs the i18n runtime. */
const LanguageSwitcherStub = defineComponent({
  name: 'UiLanguageSwitcher',
  setup: () => () => h('div', { 'data-testid': 'language-switcher-stub' }),
})

/** A signed-in client. */
const client = { id: 1, firstname: 'Ada', lastname: 'Lovelace', email: 'ada@example.test' } as ClientUser

/**
 * Mounts the header with the logo and language switcher stood in for.
 *
 * @returns The wrapper.
 */
const mountHeader = () => mountWithKit(LayoutHeader, {
  components: { AppLogo: AppLogoStub, LayoutCartButton, UiLanguageSwitcher: LanguageSwitcherStub },
  attachToBody: true,
})

describe('LayoutHeader', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
    vi.stubGlobal('useRuntimeConfig', () => ({ public: {} }))
    logoutMock.mockReset()
    window.localStorage.clear()
  })

  afterEach(() => {
    document.body.style.overflow = ''
  })

  it('renders the logo linking home', () => {
    const wrapper = mountHeader()

    expect(wrapper.find('[data-testid="layout-logo"]').attributes('href')).toBe('/en/')
    expect(wrapper.find('[data-testid="app-logo-stub"]').exists()).toBe(true)
  })

  it('lists the hosting group with its two children and the products link', () => {
    const wrapper = mountHeader()

    const children = wrapper.findAll('[data-testid="layout-nav-child"]').map((link) => link.attributes('href'))
    expect(children).toEqual(['/en/hosting', '/en/domains'])
    expect(wrapper.find('[data-testid="layout-nav-link"]').attributes('href')).toBe('/en/products')
    expect(wrapper.find('[data-testid="layout-desktop-nav"]').text()).toContain('nav.hostingDesc')
  })

  it('keeps the mobile menu closed until the burger is pressed', async () => {
    const wrapper = mountHeader()
    expect(wrapper.find('[data-testid="layout-mobile-menu"]').exists()).toBe(false)

    await wrapper.find('[data-testid="layout-mobile-toggle"]').trigger('click')

    expect(wrapper.find('[data-testid="layout-mobile-menu"]').exists()).toBe(true)
    expect(document.body.style.overflow).toBe('hidden')
  })

  it('closes the mobile menu and releases the page scroll when pressed again', async () => {
    const wrapper = mountHeader()
    const toggle = wrapper.find('[data-testid="layout-mobile-toggle"]')

    await toggle.trigger('click')
    await toggle.trigger('click')

    expect(wrapper.find('[data-testid="layout-mobile-menu"]').exists()).toBe(false)
    expect(document.body.style.overflow).toBe('')
  })

  it('expands a mobile group to show its children', async () => {
    const wrapper = mountHeader()
    await wrapper.find('[data-testid="layout-mobile-toggle"]').trigger('click')
    expect(wrapper.findAll('[data-testid="layout-mobile-child"]')).toHaveLength(0)

    await wrapper.find('[data-testid="layout-mobile-group"]').trigger('click')

    expect(wrapper.findAll('[data-testid="layout-mobile-child"]')).toHaveLength(2)
  })

  it('links a signed-out visitor to the sign-in page', async () => {
    const wrapper = mountHeader()
    await wrapper.find('[data-testid="layout-mobile-toggle"]').trigger('click')

    expect(wrapper.find('[data-testid="layout-client-area-link"]').attributes('href')).toBe('/en/client/login')
    expect(wrapper.find('[data-testid="layout-mobile-client-group"]').exists()).toBe(false)
  })

  it('links straight to the single-sign-on endpoint in SSO mode', async () => {
    vi.stubGlobal('useRuntimeConfig', () => ({ public: { authMode: 'sso' } }))
    const wrapper = mountHeader()
    await wrapper.find('[data-testid="layout-mobile-toggle"]').trigger('click')

    expect(wrapper.find('[data-testid="layout-client-area-link"]').attributes('href')).toBe('/api/portal/auth/sso/authorize')
  })

  it('shows the account links for a signed-in client', async () => {
    const wrapper = mountHeader()
    useAuthStore().user = client
    await wrapper.find('[data-testid="layout-mobile-toggle"]').trigger('click')

    const group = wrapper.find('[data-testid="layout-mobile-client-group"]')
    expect(group.text()).toContain('Ada')
    await group.trigger('click')

    const hrefs = wrapper.findAll('[data-testid="layout-client-link"]').map((link) => link.attributes('href'))
    expect(hrefs).toEqual([
      '/en/client/dashboard',
      '/en/client/services',
      '/en/client/domains',
      '/en/client/invoices',
      '/en/client/tickets',
      '/en/client/account',
    ])
  })

  it('signs the client out and goes to the sign-in page', async () => {
    const wrapper = mountHeader()
    useAuthStore().user = client
    await wrapper.find('[data-testid="layout-mobile-toggle"]').trigger('click')
    await wrapper.find('[data-testid="layout-mobile-client-group"]').trigger('click')

    await wrapper.find('[data-testid="layout-logout"]').trigger('click')
    await flushPromises()

    expect(logoutMock).toHaveBeenCalledTimes(1)
    expect(navigateTo).toHaveBeenCalledWith('/en/client/login')
    expect(wrapper.find('[data-testid="layout-mobile-menu"]').exists()).toBe(false)
  })

  it('turns translucent once the page has scrolled', async () => {
    const wrapper = mountHeader()
    const header = wrapper.find('[data-testid="layout-header"]')
    expect(header.classes()).not.toContain('backdrop-blur-lg')

    Object.defineProperty(window, 'scrollY', { value: 120, configurable: true })
    window.dispatchEvent(new Event('scroll'))
    await nextTick()

    expect(header.classes()).toContain('backdrop-blur-lg')
  })
})

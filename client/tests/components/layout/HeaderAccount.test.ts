/**
 * Unit tests for LayoutHeaderAccount: the avatar trigger, the panel with the client's name and
 * links, and sign-out through the auth store.
 */
import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'
import LayoutHeaderAccount from '~/components/layout/HeaderAccount.vue'
import { useAuthStore } from '~/stores/auth'
import type { ClientUser } from '~/types/clientuser'

/** The mocked sign-out call of the auth API. */
const logoutMock = vi.fn()

vi.mock('~/composables/apis/useAuthApi', () => ({
  /**
   * Stands in for the auth API composable so the store runs without transport.
   *
   * @returns Only the endpoints the account menu reaches for.
   */
  useAuthApi: () => ({ logout: logoutMock, ssoLogout: logoutMock, fetchUser: vi.fn() }),
}))

/** A signed-in client. */
const client = { id: 1, firstname: 'Ada', lastname: 'Lovelace', email: 'ada@example.test' } as ClientUser

/**
 * Mounts the account menu with the session store already holding a client, so the profile read
 * that runs on mount finds the user and makes no request.
 *
 * @returns The wrapper.
 */
const mountAccount = () => {
  const pinia = createPinia()
  setActivePinia(pinia)
  useAuthStore().user = client
  return mountWithKit(LayoutHeaderAccount, { attachToBody: true, pinia })
}

describe('LayoutHeaderAccount', () => {
  beforeEach(() => {
    stubTemplateGlobals()
    logoutMock.mockReset()
  })

  it('names the trigger after the client and keeps the panel closed', async () => {
    const wrapper = mountAccount()
    await wrapper.vm.$nextTick()

    const trigger = wrapper.find('[data-testid="header-account-trigger"]')
    expect(trigger.attributes('aria-label')).toBe('Ada Lovelace')
    expect(trigger.attributes('aria-expanded')).toBe('false')
    expect(wrapper.find('[data-testid="header-account-panel"]').exists()).toBe(false)
  })

  it('shows the name, e-mail and the six client-area links when opened', async () => {
    const wrapper = mountAccount()
    await wrapper.find('[data-testid="header-account-trigger"]').trigger('click')

    expect(wrapper.find('[data-testid="header-account-name"]').text()).toBe('Ada Lovelace')
    expect(wrapper.find('[data-testid="header-account-panel"]').text()).toContain('ada@example.test')
    const hrefs = ['dashboard', 'services', 'domains', 'invoices', 'tickets', 'account']
      .map((key) => wrapper.find(`[data-testid="header-account-${key}"]`).attributes('href'))
    expect(hrefs).toEqual([
      '/client/dashboard',
      '/client/services',
      '/client/domains',
      '/client/invoices',
      '/client/tickets',
      '/client/account',
    ])
  })

  it('closes when a link is chosen', async () => {
    const wrapper = mountAccount()
    await wrapper.find('[data-testid="header-account-trigger"]').trigger('click')

    await wrapper.find('[data-testid="header-account-dashboard"]').trigger('click')

    expect(wrapper.find('[data-testid="header-account-panel"]').exists()).toBe(false)
  })

  it('closes on Escape and returns focus to the trigger', async () => {
    const wrapper = mountAccount()
    await wrapper.find('[data-testid="header-account-trigger"]').trigger('click')

    await wrapper.find('[data-testid="header-account-panel"]').trigger('keydown', { key: 'Escape' })

    expect(wrapper.find('[data-testid="header-account-panel"]').exists()).toBe(false)
    expect(document.activeElement).toBe(wrapper.find('[data-testid="header-account-trigger"]').element)
  })

  it('signs the client out through the store', async () => {
    const wrapper = mountAccount()
    const store = useAuthStore()
    store.signOut = vi.fn().mockResolvedValue(undefined)
    await wrapper.find('[data-testid="header-account-trigger"]').trigger('click')

    await wrapper.find('[data-testid="header-account-sign-out"]').trigger('click')

    expect(store.signOut).toHaveBeenCalledTimes(1)
    expect(wrapper.find('[data-testid="header-account-panel"]').exists()).toBe(false)
  })

  it('falls back to the client-area label until the profile has loaded', async () => {
    const wrapper = mountAccount()
    useAuthStore().user = null
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="header-account-trigger"]').attributes('aria-label')).toBe('nav.clientArea')
  })
})

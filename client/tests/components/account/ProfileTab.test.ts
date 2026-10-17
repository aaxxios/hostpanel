/**
 * Unit tests for ProfileTab: loading, failure, the view and the edit switch.
 */
import { describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountAccount } from '~/tests/helpers/mountAccount'
import ProfileTab from '~/components/account/ProfileTab.vue'
import { useAccountStore } from '~/stores/account'
import { useClientStore } from '~/stores/client'
import type { ClientUser } from '~/types/clientuser'

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useAuthApi', () => ({ useAuthApi: () => ({}) }))
vi.mock('qrcode', () => ({ default: { toDataURL: async (): Promise<string> => '' } }))
vi.mock('~/stores/catalog', async () => {
  const { ref: vueRef } = await import('vue')
  return { useCatalogStore: () => ({ countries: vueRef([]) }) }
})

vi.stubGlobal('useAuthMode', () => ({ isSso: ref(false) }))

/** A signed-in profile with a default gateway and a language. */
const user: ClientUser = {
  id: 1, firstname: 'Jane', lastname: 'Doe', email: 'jane@example.com', permissions: 0, defaultgateway: 'stripe', language: 'ru'
}

describe('ProfileTab', () => {
  it('shows a skeleton while the profile loads', () => {
    const wrapper = mountAccount(ProfileTab)
    useClientStore().userLoading = true

    return wrapper.vm.$nextTick().then(() => {
      expect(wrapper.find('[data-testid="account-profile-loading"]').exists()).toBe(true)
    })
  })

  it('keeps the profile on screen while it is read again', async () => {
    const wrapper = mountAccount(ProfileTab)
    useClientStore().user = user
    useClientStore().userLoading = true
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="account-profile-loading"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="account-profile-edit"]').exists()).toBe(true)
  })

  it('says so when the profile could not be loaded', async () => {
    const wrapper = mountAccount(ProfileTab)
    useClientStore().userLoading = false
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="account-profile-failed"]').exists()).toBe(true)
  })

  it('shows the details with the gateway and language named', async () => {
    const wrapper = mountAccount(ProfileTab)
    useClientStore().user = user
    useClientStore().userLoading = false
    useAccountStore().gatewayMethods = [{ module: 'stripe', displayName: 'Credit card' }]
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="account-profile-view"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('Credit card')
    expect(wrapper.text()).toContain('client.profile.langRussian')
  })

  it('switches to the form on edit and back on cancel', async () => {
    const wrapper = mountAccount(ProfileTab)
    useClientStore().user = user
    useClientStore().userLoading = false
    await wrapper.vm.$nextTick()

    await wrapper.find('[data-testid="account-profile-edit"]').trigger('click')
    expect(wrapper.find('[data-testid="account-profile-form"]').exists()).toBe(true)

    await wrapper.find('[data-testid="account-profile-cancel"]').trigger('click')
    expect(wrapper.find('[data-testid="account-profile-view"]').exists()).toBe(true)
  })

  it('reports two-factor from the profile', async () => {
    const wrapper = mountAccount(ProfileTab)
    useClientStore().user = { ...user, twoFactorEnabled: true }
    useClientStore().userLoading = false
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="account-2fa-badge"]').text()).toBe('client.twoFactor.enabled')
  })
})

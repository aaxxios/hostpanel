/**
 * Smoke test for `pages/client/account/index.vue`: the page composes its header, tab strip and
 * the tab named by the query, and loads what the tabs read.
 *
 * @module tests/pages/client/account/index.test
 */

import { defineComponent, h } from 'vue'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountPage } from '~/tests/helpers/mountClient'

/** Loaders the page triggers, re-created per test. */
const loaders = {
  fetchUser: vi.fn(),
  loadCountries: vi.fn(),
  loadGatewayMethods: vi.fn(),
  loadUsers: vi.fn(),
  loadContacts: vi.fn(),
  loadEmails: vi.fn(),
  loadPaymentMethods: vi.fn(),
}

vi.mock('~/stores/client', () => ({ useClientStore: () => ({ fetchUser: loaders.fetchUser }) }))
vi.mock('~/stores/catalog', () => ({ useCatalogStore: () => ({ loadCountries: loaders.loadCountries }) }))
vi.mock('~/stores/account', () => ({
  useAccountStore: () => ({
    loadGatewayMethods: loaders.loadGatewayMethods,
    loadUsers: loaders.loadUsers,
    loadContacts: loaders.loadContacts,
    loadEmails: loaders.loadEmails,
    loadPaymentMethods: loaders.loadPaymentMethods,
  }),
}))

const { default: AccountPage } = await import('~/pages/client/account/index.vue')

/**
 * Builds a stand-in for one tab component that renders a marker.
 *
 * @param marker - The `data-testid` the stand-in renders.
 * @returns The component.
 */
const tabStub = (marker: string) => defineComponent({ render: () => h('div', { 'data-testid': marker }) })

/** The tab components the page renders, replaced by markers. */
const TAB_STUBS = {
  AccountProfileTab: tabStub('tab-details'),
  AccountContactsTab: tabStub('tab-contacts'),
  AccountTeamTab: tabStub('tab-users'),
  AccountEmailsTab: tabStub('tab-emails'),
  AccountPaymentMethodsTab: tabStub('tab-payment'),
}

/**
 * Stands in for the route with the given query.
 *
 * @param query - The `?` parameters.
 */
const stubRouteWith = (query: Record<string, string>): void => {
  vi.stubGlobal('useRoute', () => ({ params: {}, query, path: '/client/account', fullPath: '/client/account', meta: {} }))
}

beforeEach(() => {
  for (const loader of Object.values(loaders)) {
    loader.mockReset()
    loader.mockResolvedValue(undefined)
  }
  stubRouteWith({})
})

describe('client account page', () => {
  it('renders the page with the details tab by default', async () => {
    const wrapper = await mountPage(AccountPage, { components: TAB_STUBS })

    expect(wrapper.find('[data-testid="account-page"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="tab-details"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="tab-contacts"]').exists()).toBe(false)
  })

  it('opens the tab named by ?tab=', async () => {
    stubRouteWith({ tab: 'emails' })

    const wrapper = await mountPage(AccountPage, { components: TAB_STUBS })

    expect(wrapper.find('[data-testid="tab-emails"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="tab-details"]').exists()).toBe(false)
  })

  it('falls back to the details tab for an unknown ?tab=', async () => {
    stubRouteWith({ tab: 'nope' })

    const wrapper = await mountPage(AccountPage, { components: TAB_STUBS })

    expect(wrapper.find('[data-testid="tab-details"]').exists()).toBe(true)
  })

  it('loads the profile and everything the tabs read', async () => {
    await mountPage(AccountPage, { components: TAB_STUBS })

    for (const loader of Object.values(loaders)) {
      expect(loader).toHaveBeenCalledTimes(1)
    }
  })
})

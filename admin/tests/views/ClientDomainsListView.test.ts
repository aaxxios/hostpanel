/**
 * Smoke mount test for ClientDomainsListView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it, vi } from 'vitest'
import ClientDomainsListView from '@/views/ClientDomainsListView.vue'
import { mountClientView } from '../helpers/clientViews'
import { flushPromises } from '@vue/test-utils'

describe('ClientDomainsListView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientDomainsListView)

    expect(wrapper.exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientDomainsListView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })

  it('loads again from the Retry of a refused load', async () => {
    const wrapper = await mountClientView(ClientDomainsListView)
    await flushPromises()
    const fetchMock = vi.mocked(fetch)
    const before = fetchMock.mock.calls.length

    await wrapper.get('[data-testid="client-domains-error-retry"]').trigger('click')
    await flushPromises()

    expect(fetchMock.mock.calls.length).toBeGreaterThan(before)
  })
})

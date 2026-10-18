/**
 * Smoke mount test for ClientDomainsListView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientDomainsListView from '@/views/ClientDomainsListView.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientDomainsListView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientDomainsListView)

    expect(wrapper.exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientDomainsListView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })
})

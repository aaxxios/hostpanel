/**
 * Smoke mount test for ClientServicesListView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientServicesListView from '@/views/ClientServicesListView.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientServicesListView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientServicesListView)

    expect(wrapper.exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientServicesListView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })
})

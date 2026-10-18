/**
 * Smoke mount test for ClientUsersView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientUsersView from '@/views/ClientUsersView.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientUsersView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientUsersView)

    expect(wrapper.exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientUsersView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })
})

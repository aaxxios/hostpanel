/**
 * Smoke mount test for ClientEmailsView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientEmailsView from '@/views/ClientEmailsView.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientEmailsView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientEmailsView)

    expect(wrapper.exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientEmailsView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })
})

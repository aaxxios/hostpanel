/**
 * Smoke mount test for ClientTicketDetailView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientTicketDetailView from '@/views/ClientTicketDetailView.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientTicketDetailView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientTicketDetailView)

    expect(wrapper.exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientTicketDetailView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })
})

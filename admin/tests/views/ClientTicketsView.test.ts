/**
 * Smoke mount test for ClientTicketsView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientTicketsView from '@/views/ClientTicketsView.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientTicketsView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientTicketsView)

    expect(wrapper.exists()).toBe(true)
  })

  it('renders its main action', async () => {
    const wrapper = await mountClientView(ClientTicketsView)

    expect(wrapper.find('[data-testid="client-tickets-new"]').exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientTicketsView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })
})

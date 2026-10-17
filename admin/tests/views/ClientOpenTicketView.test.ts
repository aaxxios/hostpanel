/**
 * Smoke mount test for ClientOpenTicketView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientOpenTicketView from '@/views/ClientOpenTicketView.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientOpenTicketView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientOpenTicketView)

    expect(wrapper.exists()).toBe(true)
  })

  it('renders its main action', async () => {
    const wrapper = await mountClientView(ClientOpenTicketView)

    expect(wrapper.find('[data-testid="client-open-ticket-submit"]').exists()).toBe(true)
  })
})

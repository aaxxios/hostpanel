/**
 * Smoke mount test for ClientQuotesView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientQuotesView from '@/views/ClientQuotesView.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientQuotesView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientQuotesView)

    expect(wrapper.exists()).toBe(true)
  })

  it('renders its main action', async () => {
    const wrapper = await mountClientView(ClientQuotesView)

    expect(wrapper.find('[data-testid="client-quotes-create"]').exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientQuotesView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })
})

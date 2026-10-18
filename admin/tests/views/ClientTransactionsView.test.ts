/**
 * Smoke mount test for ClientTransactionsView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientTransactionsView from '@/views/ClientTransactionsView.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientTransactionsView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientTransactionsView)

    expect(wrapper.exists()).toBe(true)
  })

  it('renders its main action', async () => {
    const wrapper = await mountClientView(ClientTransactionsView)

    expect(wrapper.find('[data-testid="client-transactions-add"]').exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientTransactionsView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })
})

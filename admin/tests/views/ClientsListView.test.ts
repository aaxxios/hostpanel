/**
 * Smoke mount test for ClientsListView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientsListView from '@/views/ClientsListView.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientsListView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientsListView)

    expect(wrapper.exists()).toBe(true)
  })

  it('renders its main action', async () => {
    const wrapper = await mountClientView(ClientsListView)

    expect(wrapper.find('[data-testid="clients-list-search"]').exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientsListView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })
})

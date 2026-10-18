/**
 * Smoke mount test for ClientBillableItemsView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientBillableItemsView from '@/views/ClientBillableItemsView.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientBillableItemsView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientBillableItemsView)

    expect(wrapper.exists()).toBe(true)
  })

  it('renders its main action', async () => {
    const wrapper = await mountClientView(ClientBillableItemsView)

    expect(wrapper.find('[data-testid="client-billable-add"]').exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientBillableItemsView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })
})

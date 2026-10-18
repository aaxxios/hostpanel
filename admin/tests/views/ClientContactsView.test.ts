/**
 * Smoke mount test for ClientContactsView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientContactsView from '@/views/ClientContactsView.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientContactsView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientContactsView)

    expect(wrapper.exists()).toBe(true)
  })

  it('renders its main action', async () => {
    const wrapper = await mountClientView(ClientContactsView)

    expect(wrapper.find('[data-testid="client-contacts-add"]').exists()).toBe(true)
  })
})

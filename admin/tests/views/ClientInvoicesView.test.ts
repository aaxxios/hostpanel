/**
 * Smoke mount test for ClientInvoicesView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientInvoicesView from '@/views/ClientInvoicesView.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientInvoicesView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientInvoicesView)

    expect(wrapper.exists()).toBe(true)
  })

  it('renders its main action', async () => {
    const wrapper = await mountClientView(ClientInvoicesView)

    expect(wrapper.find('[data-testid="invoice-create-toggle"]').exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientInvoicesView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })
})

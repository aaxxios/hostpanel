/**
 * Smoke mount test for ClientSummaryView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientSummaryView from '@/views/ClientSummaryView.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientSummaryView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientSummaryView)

    expect(wrapper.exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientSummaryView, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })
})

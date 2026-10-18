/**
 * Smoke mount test for ClientLayout: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientLayout from '@/layouts/ClientLayout.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientLayout', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientLayout)

    expect(wrapper.exists()).toBe(true)
  })

  it('shows the refusal sentence from the API', async () => {
    const wrapper = await mountClientView(ClientLayout, 'Server said no')

    expect(wrapper.text()).toContain('Server said no')
  })
})

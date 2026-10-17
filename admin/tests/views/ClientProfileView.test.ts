/**
 * Smoke mount test for ClientProfileView: it renders against the real kit and settles on an API refusal.
 */
import { describe, expect, it } from 'vitest'
import ClientProfileView from '@/views/ClientProfileView.vue'
import { mountClientView } from '../helpers/clientViews'

describe('ClientProfileView', () => {
  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientProfileView)

    expect(wrapper.exists()).toBe(true)
  })
})

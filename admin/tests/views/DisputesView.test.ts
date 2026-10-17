/**
 * Smoke test for DisputesView: it mounts with the real kit and the English catalogue and renders the
 * coming-soon page under its own title.
 */
import { describe, expect, it } from 'vitest'
import DisputesView from '@/views/DisputesView.vue'
import { mountWithApp } from '../helpers/mount'

describe('DisputesView', () => {
  it('shows its title and the coming soon block', () => {
    const wrapper = mountWithApp(DisputesView)

    expect(wrapper.get('[data-testid="coming-soon-header"]').text()).toContain('List Disputes')
    expect(wrapper.get('[data-testid="coming-soon-empty"]').text()).toContain('Coming soon')
  })
})

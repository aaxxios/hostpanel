/**
 * Smoke test for OfflineCCView: it mounts with the real kit and the English catalogue and renders the
 * coming-soon page under its own title.
 */
import { describe, expect, it } from 'vitest'
import OfflineCCView from '@/views/OfflineCCView.vue'
import { mountWithApp } from '../helpers/mount'

describe('OfflineCCView', () => {
  it('shows its title and the coming soon block', () => {
    const wrapper = mountWithApp(OfflineCCView)

    expect(wrapper.get('[data-testid="coming-soon-header"]').text()).toContain('Offline Credit Card Processing')
    expect(wrapper.get('[data-testid="coming-soon-empty"]').text()).toContain('Coming soon')
  })
})

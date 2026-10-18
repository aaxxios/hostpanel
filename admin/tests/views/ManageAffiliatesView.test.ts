/**
 * Smoke test for ManageAffiliatesView: it mounts with the real kit and the English catalogue and renders the
 * coming-soon page under its own title.
 */
import { describe, expect, it } from 'vitest'
import ManageAffiliatesView from '@/views/ManageAffiliatesView.vue'
import { mountWithApp } from '../helpers/mount'

describe('ManageAffiliatesView', () => {
  it('shows its title and the coming soon block', () => {
    const wrapper = mountWithApp(ManageAffiliatesView)

    expect(wrapper.get('[data-testid="coming-soon-header"]').text()).toContain('Manage Affiliates')
    expect(wrapper.get('[data-testid="coming-soon-empty"]').text()).toContain('Coming soon')
  })
})

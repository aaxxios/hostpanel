/**
 * Smoke test for ServiceAddonsView: it mounts with the real kit and the English catalogue and renders the
 * coming-soon page under its own title.
 */
import { describe, expect, it } from 'vitest'
import ServiceAddonsView from '@/views/ServiceAddonsView.vue'
import { mountWithApp } from '../helpers/mount'

describe('ServiceAddonsView', () => {
  it('shows its title and the coming soon block', () => {
    const wrapper = mountWithApp(ServiceAddonsView)

    expect(wrapper.get('[data-testid="coming-soon-header"]').text()).toContain('Service Addons')
    expect(wrapper.get('[data-testid="coming-soon-empty"]').text()).toContain('Coming soon')
  })
})

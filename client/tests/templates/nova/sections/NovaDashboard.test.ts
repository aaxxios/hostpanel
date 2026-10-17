/**
 * Unit tests for NovaDashboard: the full section and the compact window-only form.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaDashboard from '~/templates/nova/sections/NovaDashboard.vue'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'

describe('NovaDashboard', () => {
  beforeEach(() => {
    stubTemplateGlobals()
  })

  it('renders the copy, the sign-in link and the window', () => {
    const wrapper = mountWithKit(NovaDashboard)

    expect(wrapper.find('h2').text()).toBe('nova.dashboard.title')
    expect(wrapper.find('[data-testid="nova-dashboard-cta"]').attributes('href')).toBe('/client/login')
    expect(wrapper.find('[aria-hidden="true"]').exists()).toBe(true)
  })

  it('renders only the window when compact, with no heading', () => {
    const wrapper = mountWithKit(NovaDashboard, { props: { compact: true } })

    expect(wrapper.find('h2').exists()).toBe(false)
    expect(wrapper.find('[data-testid="nova-dashboard-cta"]').exists()).toBe(false)
    expect(wrapper.findAll('li').length).toBeGreaterThan(0)
  })
})

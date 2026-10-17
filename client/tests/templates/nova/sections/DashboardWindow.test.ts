/**
 * Unit tests for DashboardWindow: the decorative control-panel drawing.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import DashboardWindow from '~/templates/nova/sections/DashboardWindow.vue'
import { DASHBOARD_ROWS } from '~/templates/nova/content'

describe('DashboardWindow', () => {
  it('is hidden from assistive technology', () => {
    expect(mountWithKit(DashboardWindow).attributes('aria-hidden')).toBe('true')
  })

  it('draws one row per dashboard area with its label and value key', () => {
    const wrapper = mountWithKit(DashboardWindow)
    const rows = wrapper.findAll('li')

    expect(rows).toHaveLength(DASHBOARD_ROWS.length)
    expect(rows[0]!.text()).toContain(DASHBOARD_ROWS[0]!.labelKey)
    expect(rows[0]!.text()).toContain(DASHBOARD_ROWS[0]!.valueKey)
  })
})

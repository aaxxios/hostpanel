/**
 * Unit tests for UsageBar: the label, the "used / allowance" line and the bar's percentage.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import UsageBar from '~/components/client/services/UsageBar.vue'

describe('UsageBar', () => {
  it('shows the label and the usage line', () => {
    const wrapper = mountClient(UsageBar, { props: { label: 'Disk', used: '256', limit: '1024' } })

    expect(wrapper.text()).toContain('Disk')
    expect(wrapper.text()).toContain('client.services.usageOf')
  })

  it('fills the bar by the share used', () => {
    const wrapper = mountClient(UsageBar, { props: { label: 'Disk', used: '256', limit: '1024' } })

    expect(wrapper.find('[data-testid="ui-progress"]').attributes('aria-valuenow')).toBe('25')
  })

  it('caps the bar at full', () => {
    const wrapper = mountClient(UsageBar, { props: { label: 'Disk', used: '5000', limit: '1024' } })

    expect(wrapper.find('[data-testid="ui-progress"]').attributes('aria-valuenow')).toBe('100')
  })

  it('draws the bar in the tone it is given', () => {
    const wrapper = mountClient(UsageBar, { props: { label: 'Bandwidth', used: '1', limit: '2', tone: 'secondary' } })

    expect(wrapper.find('[data-testid="ui-progress-bar"]').classes()).toContain('bg-secondary-500')
  })
})

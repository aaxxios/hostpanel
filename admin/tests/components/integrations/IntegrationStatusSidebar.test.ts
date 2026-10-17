/**
 * Mount tests for IntegrationStatusSidebar: the three states (testing, result, idle) and the
 * optional rows and hint.
 */
import { describe, expect, it } from 'vitest'
import IntegrationStatusSidebar from '@/components/integrations/IntegrationStatusSidebar.vue'
import { mountWithApp } from '../../helpers/mount'

/**
 * Mounts the sidebar.
 *
 * @param props - Props to change.
 * @returns The wrapper.
 */
const mountSidebar = (props: Record<string, unknown> = {}) =>
  mountWithApp(IntegrationStatusSidebar, { props: { lastTestedAt: null, testResult: null, ...props } })

describe('IntegrationStatusSidebar', () => {
  it('shows "not tested yet" and "never" when nothing was tested', () => {
    const wrapper = mountSidebar()

    const idle = wrapper.get('[data-testid="integration-status-idle"]')
    expect(idle.text()).toContain('Not tested yet')
    expect(idle.text()).toContain('Never')
  })

  it('shows the previous test when there is a persisted time', () => {
    const wrapper = mountSidebar({ lastTestedAt: new Date(Date.now() - 5 * 60000).toISOString() })

    const idle = wrapper.get('[data-testid="integration-status-idle"]')
    expect(idle.text()).toContain('Previously tested OK')
    expect(idle.text()).toContain('minutes ago')
  })

  it('shows the in-flight state first', () => {
    const wrapper = mountSidebar({ testing: true, testResult: { success: true, message: 'ok' } })

    expect(wrapper.find('[data-testid="integration-status-testing"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="integration-status-result"]').exists()).toBe(false)
  })

  it('shows the verdict, the message and the detail rows of a result', () => {
    const wrapper = mountSidebar({
      testResult: { success: false, message: 'Bad key.', testedAt: '2026-10-14T10:00:00Z', durationMs: 2400 },
      endpointHost: 'pg.example.com',
    })

    const result = wrapper.get('[data-testid="integration-status-result"]')
    expect(result.text()).toContain('Connection Failed')
    expect(result.text()).toContain('Bad key.')
    expect(result.text()).toContain('pg.example.com')
    expect(result.text()).toContain('2.4 s')
  })

  it('shows the hint callout only when there is one', () => {
    expect(mountSidebar().find('[data-testid="integration-status-hint"]').exists()).toBe(false)
    expect(mountSidebar({ hint: 'Add the webhook.' }).get('[data-testid="integration-status-hint"]').text()).toContain('Add the webhook.')
  })
})

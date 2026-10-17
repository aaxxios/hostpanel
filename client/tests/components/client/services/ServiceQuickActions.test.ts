/**
 * Unit tests for ServiceQuickActions: which shortcuts show and the event each raises.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import ServiceQuickActions from '~/components/client/services/ServiceQuickActions.vue'

describe('ServiceQuickActions', () => {
  it('offers the control-panel shortcut only on a known server', () => {
    const known = mountClient(ServiceQuickActions, { props: { hasServer: true, ssoLoading: false } })
    const unknown = mountClient(ServiceQuickActions, { props: { hasServer: false, ssoLoading: false } })

    expect(known.find('[data-testid="service-quick-cpanel"]').exists()).toBe(true)
    expect(unknown.find('[data-testid="service-quick-cpanel"]').exists()).toBe(false)
  })

  it('raises the event of the shortcut that was clicked', async () => {
    const wrapper = mountClient(ServiceQuickActions, { props: { hasServer: true, ssoLoading: false } })

    await wrapper.find('[data-testid="service-quick-cpanel"]').trigger('click')
    await wrapper.find('[data-testid="service-quick-invoices"]').trigger('click')
    await wrapper.find('[data-testid="service-quick-cancel"]').trigger('click')

    expect(wrapper.emitted('cpanel')).toHaveLength(1)
    expect(wrapper.emitted('invoices')).toHaveLength(1)
    expect(wrapper.emitted('cancel')).toHaveLength(1)
  })
})

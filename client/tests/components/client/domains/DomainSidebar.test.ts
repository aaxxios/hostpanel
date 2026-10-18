/**
 * Unit tests for DomainSidebar: the panels, the lock on a pending domain and the renew action.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import DomainSidebar from '~/components/client/domains/DomainSidebar.vue'
import { DomainTab } from '~/types/domaintab'

describe('DomainSidebar', () => {
  it('marks the active panel', () => {
    const wrapper = mountClient(DomainSidebar, { props: { activeTab: DomainTab.Lock, isPending: false } })

    expect(wrapper.find('[data-testid="domain-tab-lock"]').classes()).toContain('font-medium')
    expect(wrapper.find('[data-testid="domain-tab-overview"]').classes()).not.toContain('font-medium')
  })

  it('emits the panel that was clicked', async () => {
    const wrapper = mountClient(DomainSidebar, { props: { activeTab: DomainTab.Overview, isPending: false } })

    await wrapper.find('[data-testid="domain-tab-nameservers"]').trigger('click')

    expect(wrapper.emitted('update:activeTab')).toEqual([[DomainTab.Nameservers]])
  })

  it('locks every panel but the overview while the domain is pending', async () => {
    const wrapper = mountClient(DomainSidebar, { props: { activeTab: DomainTab.Overview, isPending: true } })

    expect(wrapper.find('[data-testid="domain-tab-contact"]').attributes('disabled')).toBeDefined()
    expect(wrapper.find('[data-testid="domain-tab-overview"]').attributes('disabled')).toBeUndefined()

    await wrapper.find('[data-testid="domain-tab-contact"]').trigger('click')
    expect(wrapper.emitted('update:activeTab')).toBeUndefined()
  })

  it('opens the renew panel from the actions', async () => {
    const wrapper = mountClient(DomainSidebar, { props: { activeTab: DomainTab.Overview, isPending: false } })

    await wrapper.find('[data-testid="domain-action-renew"]').trigger('click')

    expect(wrapper.emitted('update:activeTab')).toEqual([[DomainTab.Renew]])
  })

  it('links to registering another domain', () => {
    const wrapper = mountClient(DomainSidebar, { props: { activeTab: DomainTab.Overview, isPending: false } })

    expect(wrapper.find('[data-testid="domain-action-register"]').attributes('href')).toBe('/domains')
  })
})

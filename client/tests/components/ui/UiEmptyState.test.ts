/**
 * Unit tests for UiEmptyState: the title, the optional description and icon, and the action slot.
 */
import { describe, expect, it } from 'vitest'
import { Inbox } from 'lucide-vue-next'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiEmptyState from '~/components/ui/UiEmptyState.vue'

describe('UiEmptyState', () => {
  it('renders the title as a heading', () => {
    const wrapper = mountWithKit(UiEmptyState, { props: { title: 'No invoices yet' } })

    expect(wrapper.find('h3').text()).toBe('No invoices yet')
    expect(wrapper.find('p').exists()).toBe(false)
    expect(wrapper.find('svg').exists()).toBe(false)
  })

  it('renders the description and the icon', () => {
    const wrapper = mountWithKit(UiEmptyState, {
      props: { title: 'No invoices yet', description: 'They will appear here.', icon: Inbox },
    })

    expect(wrapper.find('p').text()).toBe('They will appear here.')
    expect(wrapper.find('svg').exists()).toBe(true)
  })

  it('renders the action slot', () => {
    const wrapper = mountWithKit(UiEmptyState, { props: { title: 'Empty' }, slots: { default: 'Create one' } })

    expect(wrapper.text()).toContain('Create one')
  })

  it('has a stable test id on its root', () => {
    const wrapper = mountWithKit(UiEmptyState, { props: { title: 'Empty' } })

    expect(wrapper.attributes('data-testid')).toBe('ui-empty-state')
  })
})

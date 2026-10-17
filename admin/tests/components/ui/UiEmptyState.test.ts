/**
 * Unit tests for UiEmptyState: title, description and the icon/actions slots.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'

describe('UiEmptyState', () => {
  it('renders the title as a heading and the description', () => {
    const wrapper = mount(UiEmptyState, { props: { title: 'No invoices', description: 'Create the first one' } })

    expect(wrapper.get('h3').text()).toBe('No invoices')
    expect(wrapper.text()).toContain('Create the first one')
  })

  it('omits the description when none is given', () => {
    const wrapper = mount(UiEmptyState, { props: { title: 'Nothing' } })

    expect(wrapper.find('p').exists()).toBe(false)
  })

  it('renders the icon and actions slots', () => {
    const wrapper = mount(UiEmptyState, {
      props: { title: 'Nothing' },
      slots: { icon: '<i data-testid="icon" />', actions: '<b data-testid="action" />' },
    })

    expect(wrapper.find('[data-testid="icon"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="action"]').exists()).toBe(true)
  })
})

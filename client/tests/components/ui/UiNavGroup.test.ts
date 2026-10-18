/**
 * Unit tests for UiNavGroup: the title, the icon and the menu slot.
 */
import { describe, expect, it } from 'vitest'
import { Settings } from 'lucide-vue-next'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiNavGroup from '~/components/ui/UiNavGroup.vue'

describe('UiNavGroup', () => {
  it('shows the title and names the menu after it', () => {
    const wrapper = mountWithKit(UiNavGroup, { props: { title: 'Manage' }, slots: { default: '<span>entry</span>' } })

    expect(wrapper.text()).toContain('Manage')
    expect(wrapper.find('nav').attributes('aria-label')).toBe('Manage')
    expect(wrapper.find('nav').text()).toBe('entry')
  })

  it('draws the icon only when one is given', () => {
    const without = mountWithKit(UiNavGroup, { props: { title: 'Manage' } })
    const withIcon = mountWithKit(UiNavGroup, { props: { title: 'Manage', icon: Settings } })

    expect(without.find('svg').exists()).toBe(false)
    expect(withIcon.find('svg').exists()).toBe(true)
  })
})

/**
 * Unit tests for UiListItem: a plain item, and an item led by an icon.
 */
import { describe, expect, it } from 'vitest'
import { Check } from 'lucide-vue-next'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiListItem from '~/components/ui/UiListItem.vue'

describe('UiListItem', () => {
  it('is a li with its slot and no icon by default', () => {
    const wrapper = mountWithKit(UiListItem, { slots: { default: 'Free SSL' } })

    expect(wrapper.element.tagName).toBe('LI')
    expect(wrapper.text()).toBe('Free SSL')
    expect(wrapper.find('svg').exists()).toBe(false)
    expect(wrapper.classes()).not.toContain('flex')
  })

  it('leads with the icon in a flex row', () => {
    const wrapper = mountWithKit(UiListItem, { props: { icon: Check }, slots: { default: 'Free SSL' } })

    expect(wrapper.find('svg').exists()).toBe(true)
    expect(wrapper.text()).toBe('Free SSL')
    expect(wrapper.classes()).toEqual(expect.arrayContaining(['flex', 'items-start']))
  })
})

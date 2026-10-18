/**
 * Unit tests for UiIcon: shapes drawn from markup without v-html, hostile markup dropped.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiIcon from '@/components/ui/UiIcon.vue'

describe('UiIcon', () => {
  it('draws the shapes of the markup inside an svg', () => {
    const wrapper = mount(UiIcon, { props: { markup: '<path d="M1 1"/><circle cx="2" cy="2" r="1"/>' } })

    expect(wrapper.element.tagName.toLowerCase()).toBe('svg')
    expect(wrapper.find('path').attributes('d')).toBe('M1 1')
    expect(wrapper.find('circle').exists()).toBe(true)
  })

  it('never renders scripts or event handlers from the markup', () => {
    const wrapper = mount(UiIcon, { props: { markup: '<script>alert(1)</script><path d="M1 1" onclick="x()"/>' } })

    expect(wrapper.find('script').exists()).toBe(false)
    expect(wrapper.find('path').attributes('onclick')).toBeUndefined()
  })

  it('is hidden from assistive technology and sized', () => {
    const wrapper = mount(UiIcon, { props: { markup: '', size: 'lg' } })

    expect(wrapper.attributes('aria-hidden')).toBe('true')
    expect(wrapper.classes()).toContain('w-5')
  })
})

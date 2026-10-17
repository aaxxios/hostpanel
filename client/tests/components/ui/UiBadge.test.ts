/**
 * Unit tests for UiBadge: tone colours, sizes, the pill shape and the leading icon.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiBadge from '~/components/ui/UiBadge.vue'
import { UiSize, UiTone } from '~/utils/ui/enums'

describe('UiBadge', () => {
  it('renders its slot in a span', () => {
    const wrapper = mountWithKit(UiBadge, { slots: { default: 'Active' } })

    expect(wrapper.element.tagName).toBe('SPAN')
    expect(wrapper.text()).toBe('Active')
  })

  it.each([
    [UiTone.Success, 'bg-green-100'],
    [UiTone.Danger, 'bg-red-100'],
    [UiTone.Warning, 'bg-yellow-100'],
    [UiTone.Info, 'bg-blue-100'],
    [UiTone.Primary, 'bg-primary-100'],
    [UiTone.Default, 'bg-gray-100'],
  ])('paints the %s tone with %s', (variant, expected) => {
    const wrapper = mountWithKit(UiBadge, { props: { variant } })

    expect(wrapper.classes()).toContain(expected)
  })

  it('applies the size classes', () => {
    const wrapper = mountWithKit(UiBadge, { props: { size: UiSize.Sm } })

    expect(wrapper.classes()).toContain('text-xs')
  })

  it('is a pill when rounded and a small radius otherwise', () => {
    expect(mountWithKit(UiBadge, { props: { rounded: true } }).classes()).toContain('rounded-full')
    expect(mountWithKit(UiBadge).classes()).toContain('rounded')
  })

  it('shows the icon when one is named', () => {
    const wrapper = mountWithKit(UiBadge, { props: { icon: 'lucide:check' } })

    expect(wrapper.find('i[data-icon="lucide:check"]').exists()).toBe(true)
  })
})

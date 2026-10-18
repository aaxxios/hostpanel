/**
 * Unit tests for UiProgress: the accessible value, the clamping and the width custom property.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiProgress from '~/components/ui/UiProgress.vue'
import { UiTone } from '~/utils/ui/enums'

describe('UiProgress', () => {
  it('exposes the value and the name to assistive technology', () => {
    const wrapper = mountWithKit(UiProgress, { props: { value: 40, label: 'Disk' } })
    const track = wrapper.find('[data-testid="ui-progress"]')

    expect(track.attributes('role')).toBe('progressbar')
    expect(track.attributes('aria-label')).toBe('Disk')
    expect(track.attributes('aria-valuenow')).toBe('40')
  })

  it('writes the width as a custom property, not as a style attribute value of its own', () => {
    const wrapper = mountWithKit(UiProgress, { props: { value: 40, label: 'Disk' } })
    const track = wrapper.find('[data-testid="ui-progress"]').element as HTMLElement

    expect(track.style.getPropertyValue('--ui-progress')).toBe('40%')
    expect(wrapper.find('[data-testid="ui-progress-bar"]').classes()).toContain('w-[var(--ui-progress,0%)]')
  })

  it.each([
    [150, '100'],
    [-5, '0'],
    [Number.NaN, '0'],
  ])('clamps %s to %s', (value, expected) => {
    const wrapper = mountWithKit(UiProgress, { props: { value, label: 'Disk' } })

    expect(wrapper.find('[data-testid="ui-progress"]').attributes('aria-valuenow')).toBe(expected)
  })

  it('follows the value when it changes', async () => {
    const wrapper = mountWithKit(UiProgress, { props: { value: 10, label: 'Disk' } })

    await wrapper.setProps({ value: 75 })

    const track = wrapper.find('[data-testid="ui-progress"]').element as HTMLElement
    expect(track.style.getPropertyValue('--ui-progress')).toBe('75%')
  })

  it('colours the fill by tone', () => {
    const wrapper = mountWithKit(UiProgress, { props: { value: 10, label: 'Disk', tone: UiTone.Danger } })

    expect(wrapper.find('[data-testid="ui-progress-bar"]').classes()).toContain('bg-red-500')
  })
})

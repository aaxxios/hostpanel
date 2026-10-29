/**
 * Unit tests for UiProgress: the accessible value and value text, the clamping and the bar width
 * rendered as markup.
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

  it('reads out the value text when given one, and nothing extra otherwise', () => {
    const withText = mountWithKit(UiProgress, { props: { value: 25, label: 'Disk', valueText: '256 MB / 1024 MB' } })
    const without = mountWithKit(UiProgress, { props: { value: 25, label: 'Disk' } })

    expect(withText.find('[data-testid="ui-progress"]').attributes('aria-valuetext')).toBe('256 MB / 1024 MB')
    expect(without.find('[data-testid="ui-progress"]').attributes('aria-valuetext')).toBeUndefined()
  })

  it('renders the width in the markup, without any inline style', () => {
    const wrapper = mountWithKit(UiProgress, { props: { value: 40, label: 'Disk' } })

    expect(wrapper.find('[data-testid="ui-progress-bar"]').attributes('width')).toBe('40%')
    expect(wrapper.html()).not.toContain('style=')
  })

  it('draws the same width on the server as it announces', async () => {
    const { renderToString } = await import('vue/server-renderer')
    const { createSSRApp, h } = await import('vue')
    const html = await renderToString(createSSRApp({ render: () => h(UiProgress, { value: 62.4, label: 'Disk' }) }))

    expect(html).toContain('aria-valuenow="62"')
    expect(html).toContain('width="62%"')
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

    expect(wrapper.find('[data-testid="ui-progress-bar"]').attributes('width')).toBe('75%')
    expect(wrapper.find('[data-testid="ui-progress"]').attributes('aria-valuenow')).toBe('75')
  })

  it('colours the fill by tone', () => {
    const wrapper = mountWithKit(UiProgress, { props: { value: 10, label: 'Disk', tone: UiTone.Danger } })

    expect(wrapper.find('[data-testid="ui-progress-bar"]').classes()).toContain('fill-red-500')
  })
})

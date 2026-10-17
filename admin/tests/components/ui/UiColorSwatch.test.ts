/**
 * Unit tests for UiColorSwatch: a filled block, a button sample and coloured text, all by SVG
 * attributes (no inline style).
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiColorSwatch from '@/components/ui/UiColorSwatch.vue'

/**
 * Reads the drawing width out of an SVG `viewBox` attribute.
 *
 * @param viewBox - The attribute value, e.g. `0 0 48 32`.
 * @returns The third number (the width).
 */
const widthOf = (viewBox: string | undefined): number => Number((viewBox ?? '').split(' ')[2])

describe('UiColorSwatch', () => {
  it('fills a block with the colour and names it for assistive technology', () => {
    const wrapper = mount(UiColorSwatch, { props: { color: '#0ea5e9', title: '#0ea5e9' } })

    expect(wrapper.get('rect').attributes('fill')).toBe('#0ea5e9')
    expect(wrapper.attributes('aria-label')).toBe('#0ea5e9')
    expect(wrapper.find('text').exists()).toBe(false)
    expect(wrapper.attributes('style')).toBeUndefined()
  })

  it('draws a button sample: filled rectangle with a coloured label', () => {
    const wrapper = mount(UiColorSwatch, { props: { color: '#111111', textColor: '#ffffff', label: 'Button' } })

    expect(wrapper.get('rect').attributes('fill')).toBe('#111111')
    expect(wrapper.get('text').text()).toBe('Button')
    expect(wrapper.get('text').attributes('fill')).toBe('#ffffff')
  })

  it('draws text only when there is no colour, in the surrounding colour by default', () => {
    const wrapper = mount(UiColorSwatch, { props: { label: 'Link' } })

    expect(wrapper.find('rect').exists()).toBe(false)
    expect(wrapper.get('text').attributes('fill')).toBe('currentColor')
  })

  it('widens the drawing for a longer label', () => {
    const short = mount(UiColorSwatch, { props: { label: 'A' } })
    const long = mount(UiColorSwatch, { props: { label: 'A much longer caption' } })

    expect(widthOf(long.attributes('viewBox'))).toBeGreaterThan(widthOf(short.attributes('viewBox')))
  })
})

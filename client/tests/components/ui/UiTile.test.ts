/**
 * Unit tests for UiTile: the label slot, the optional icon, the size steps and the round shape.
 */
import { describe, expect, it } from 'vitest'
import { defineComponent, h, markRaw } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiTile from '~/components/ui/UiTile.vue'
import { UiTileSize, UiTone } from '~/utils/ui/enums'

/** Icon stand-in: a component that renders an `svg` carrying the size it was given; marked raw because a test-utils prop would otherwise make it reactive. */
const IconStub = markRaw(defineComponent({
  props: { size: { type: Number, default: 0 } },
  setup(props) {
    return () => h('svg', { 'data-size': props.size })
  },
}))

describe('UiTile', () => {
  it('renders its label centred in a soft square', () => {
    const wrapper = mountWithKit(UiTile, { slots: { default: 'Web' } })

    expect(wrapper.text()).toBe('Web')
    expect(wrapper.classes()).toEqual(expect.arrayContaining(['grid', 'place-items-center', 'rounded-xl', 'h-8', 'w-8']))
  })

  it('carries no colour of its own, so the screen paints it', () => {
    const wrapper = mountWithKit(UiTile)

    expect(wrapper.classes().some((name) => name.startsWith('bg-'))).toBe(false)
  })

  it.each([
    [UiTileSize.Sm, 'h-5'],
    [UiTileSize.Md, 'h-8'],
    [UiTileSize.Lg, 'h-10'],
    [UiTileSize.Xl, 'h-12'],
  ])('maps size %s to %s', (size, expected) => {
    expect(mountWithKit(UiTile, { props: { size } }).classes()).toContain(expected)
  })

  it('is fully round when asked', () => {
    const wrapper = mountWithKit(UiTile, { props: { rounded: true } })

    expect(wrapper.classes()).toContain('rounded-full')
    expect(wrapper.classes()).not.toContain('rounded-xl')
  })

  it('draws the icon at the size step', () => {
    const wrapper = mountWithKit(UiTile, { props: { icon: IconStub, size: UiTileSize.Xl } })

    expect(wrapper.find('svg').attributes('data-size')).toBe('24')
  })

  it.each([
    [UiTone.Success, 'bg-green-500/10'],
    [UiTone.Danger, 'bg-red-500/10'],
    [UiTone.Primary, 'bg-primary-500/10'],
  ])('paints the %s tone with %s', (tone, expected) => {
    const wrapper = mountWithKit(UiTile, { props: { icon: IconStub, tone } })

    expect(wrapper.classes()).toContain(expected)
  })

  it('adds no tint without a tone so the caller can paint it', () => {
    const wrapper = mountWithKit(UiTile, { attrs: { class: 'bg-cyan-500/20' } })

    expect(wrapper.classes().filter((c) => c.startsWith('bg-') && c !== 'bg-cyan-500/20')).toHaveLength(0)
  })

  it('exposes a test id', () => {
    expect(mountWithKit(UiTile).find('[data-testid="ui-tile"]').exists()).toBe(true)
  })
})

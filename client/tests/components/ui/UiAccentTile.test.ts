/**
 * Unit tests for UiAccentTile: the icon, the accent classes and the size steps.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiAccentTile from '~/components/ui/UiAccentTile.vue'
import { UiAccentTileSize } from '~/utils/ui/accent'

describe('UiAccentTile', () => {
  it('shows the named icon in the accent colour', () => {
    const wrapper = mountWithKit(UiAccentTile, { props: { icon: 'mdi:school' } })
    const icon = wrapper.find('[data-icon="mdi:school"]')

    expect(icon.exists()).toBe(true)
    expect(icon.classes()).toContain('dark:text-[color:color-mix(in_srgb,var(--accent)_75%,white)]')
  })

  it('tints the square with the accent', () => {
    const wrapper = mountWithKit(UiAccentTile, { props: { icon: 'mdi:cube' } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['w-14', 'h-14']))
    expect(wrapper.classes().some((name) => name.includes('var(--accent)'))).toBe(true)
  })

  it.each([
    [UiAccentTileSize.Xs, 'w-6'],
    [UiAccentTileSize.Sm, 'w-12'],
    [UiAccentTileSize.Lg, 'w-16'],
    [UiAccentTileSize.Xl, 'w-20'],
  ])('sizes the %s tile', (size, expected) => {
    expect(mountWithKit(UiAccentTile, { props: { icon: 'mdi:cube', size } }).classes()).toContain(expected)
  })
})

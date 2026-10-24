/**
 * Unit tests for UiLogoWall: the balanced grid, the compact row and the looping marquee, the
 * logo or wordmark of every item, and the hidden second copy of the marquee.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiLogoWall from '~/components/ui/UiLogoWall.vue'
import { UiLogoWallLayout } from '~/utils/ui/logoWall'
import type { LogoWallItem } from '~/utils/ui/logoWall'
import { balancedColumns } from '~/utils/balancedGrid'

/**
 * Builds `count` items, the odd ones with a logo and the even ones without.
 *
 * @param count How many items.
 * @returns The items.
 */
const items = (count: number): LogoWallItem[] =>
  Array.from({ length: count }, (_, index) => ({
    id: index + 1,
    name: `Example Co ${index + 1}`,
    logoUrl: index % 2 === 0 ? `https://example.test/${index + 1}.svg` : null,
    url: null,
  }))

describe('UiLogoWall', () => {
  it('lays the logos out in the shared balanced grid by default', () => {
    const wrapper = mountWithKit(UiLogoWall, { props: { items: items(5), testidPrefix: 'partner' } })

    expect(wrapper.classes()).toContain('ui-balanced-grid')
    expect(wrapper.attributes('data-columns')).toBe(String(balancedColumns(5)))
    expect(wrapper.find('[data-testid="partner-5"]').exists()).toBe(true)
  })

  it('draws a logo for an item that has one and a wordmark for one that has not', () => {
    const wrapper = mountWithKit(UiLogoWall, { props: { items: items(2), testidPrefix: 'partner' } })

    expect(wrapper.find('[data-testid="partner-logo-1"]').attributes('src')).toBe('https://example.test/1.svg')
    expect(wrapper.find('[data-testid="partner-logo-2"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="partner-2"]').text()).toBe('Example Co 2')
  })

  it('renders a centred wrapping row for the compact layout', () => {
    const wrapper = mountWithKit(UiLogoWall, { props: { items: items(3), testidPrefix: 'trusted', layout: UiLogoWallLayout.Row } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['flex', 'flex-wrap', 'justify-center']))
    expect(wrapper.find('[data-testid="trusted-3"]').exists()).toBe(true)
  })

  it('loops the marquee through a second copy that assistive technology and reduced motion skip', () => {
    const wrapper = mountWithKit(UiLogoWall, { props: { items: items(4), testidPrefix: 'partner', layout: UiLogoWallLayout.Marquee } })

    expect(wrapper.find('.animate-marquee').classes()).toContain('hover:[animation-play-state:paused]')
    expect(wrapper.classes().join(' ')).toContain('mask-image')
    expect(wrapper.find('[data-testid="partner-logo-1"]').classes()).toContain('group-hover:grayscale-0')
    expect(wrapper.findAll('[data-testid="partner-1"]')).toHaveLength(2)
    const hidden = wrapper.findAll('[aria-hidden="true"]')
    expect(hidden).toHaveLength(4)
    expect(hidden[0]?.classes()).toContain('motion-reduce:hidden')
  })
})

/**
 * Unit tests for UiLogoWallItem: a logo in the muted style that gains colour on hover, a plain
 * wordmark when there is no logo, and a link only when the partner has a website.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiLogoWallItem from '~/components/ui/UiLogoWallItem.vue'
import type { LogoWallItem } from '~/utils/ui/logoWall'

/**
 * Builds an item.
 *
 * @param overrides Fields that differ from the defaults.
 * @returns The logo item.
 */
const item = (overrides: Partial<LogoWallItem> = {}): LogoWallItem => ({
  id: 1,
  name: 'Example Co',
  logoUrl: 'https://example.test/logo.svg',
  url: null,
  ...overrides,
})

describe('UiLogoWallItem', () => {
  it('draws the logo muted, with colour on hover and focus', () => {
    const wrapper = mountWithKit(UiLogoWallItem, { props: { item: item(), testidPrefix: 'partner' } })

    const logo = wrapper.find('[data-testid="partner-logo-1"]')
    expect(logo.attributes('src')).toBe('https://example.test/logo.svg')
    expect(logo.attributes('alt')).toBe('Example Co')
    expect(logo.classes()).toEqual(expect.arrayContaining(['grayscale', 'opacity-60', 'group-hover:grayscale-0', 'group-focus-visible:grayscale-0', 'dark:brightness-125']))
  })

  it('shows the name as a plain wordmark, not a card, when there is no logo', () => {
    const wrapper = mountWithKit(UiLogoWallItem, { props: { item: item({ logoUrl: null }), testidPrefix: 'trusted' } })

    expect(wrapper.find('[data-testid="trusted-logo-1"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="trusted-1"]').text()).toBe('Example Co')
    expect(wrapper.find('[data-testid="ui-card"]').exists()).toBe(false)
  })

  it('links to the website only when there is one', () => {
    const linked = mountWithKit(UiLogoWallItem, { props: { item: item({ url: 'https://partner.example' }), testidPrefix: 'partner' } })
    const plain = mountWithKit(UiLogoWallItem, { props: { item: item(), testidPrefix: 'partner' } })

    expect(linked.find('[data-testid="partner-1"]').attributes('href')).toBe('https://partner.example')
    expect(plain.find('[data-testid="partner-1"]').attributes('href')).toBeUndefined()
  })
})

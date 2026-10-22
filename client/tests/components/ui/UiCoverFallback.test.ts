/**
 * Unit tests for UiCoverFallback: the initials, the site type icon chosen from the slug, and the
 * decorative pattern.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiCoverFallback from '~/components/ui/UiCoverFallback.vue'
import { SiteType } from '~/utils/ui/siteType'

describe('UiCoverFallback', () => {
  it('shows the initials of the first two words of the title', () => {
    const wrapper = mountWithKit(UiCoverFallback, { props: { title: 'Bluebird Online Shop', slug: 'bluebird-shop' } })

    expect(wrapper.find('[data-testid="ui-cover-fallback-initials"]').text()).toBe('BO')
  })

  it('shows a single letter for a one-word title', () => {
    const wrapper = mountWithKit(UiCoverFallback, { props: { title: 'Blog', slug: 'blog' } })

    expect(wrapper.find('[data-testid="ui-cover-fallback-initials"]').text()).toBe('B')
  })

  it('picks the icon for the kind of site from the slug', () => {
    const shop = mountWithKit(UiCoverFallback, { props: { title: 'Bluebird', slug: 'bluebird-shop' } })
    const other = mountWithKit(UiCoverFallback, { props: { title: 'Plain', slug: 'plain' } })

    expect(shop.find('[data-testid="ui-cover-fallback-icon"]').attributes('data-site-type')).toBe(SiteType.Shop)
    expect(other.find('[data-testid="ui-cover-fallback-icon"]').attributes('data-site-type')).toBe(SiteType.Website)
  })

  it('draws a decorative pattern hidden from assistive technology', () => {
    const wrapper = mountWithKit(UiCoverFallback, { props: { title: 'Plain', slug: 'plain' } })

    expect(wrapper.findAll('[aria-hidden="true"]').length).toBeGreaterThan(0)
  })
})

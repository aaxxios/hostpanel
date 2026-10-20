/**
 * Unit tests for AuroraHero: the single page headline, both calls to action and the operator-set badge and figures.
 */
import { describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import AuroraHero from '~/templates/aurora/sections/AuroraHero.vue'
import { stubAuroraGlobals } from '../auroraGlobals'

describe('AuroraHero', () => {
  it('renders the page headline as the only h1, with the accented phrase', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraHero)

    expect(wrapper.findAll('h1')).toHaveLength(1)
    expect(wrapper.find('[data-testid="hero-title"]').text()).toContain('aurora.hero.titleAccent')
  })

  it('links the two calls to action to the domain search and the contact page', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraHero)

    expect(wrapper.find('[data-testid="hero-cta-primary"]').attributes('href')).toBe('/domains')
    expect(wrapper.find('[data-testid="hero-cta-secondary"]').attributes('href')).toBe('/contact')
  })

  it('shows the badge and the figures the operator configured', () => {
    stubAuroraGlobals()
    const settings: Record<string, string> = {
      'portal.hero.badge': 'Fast and calm',
      'portal.stats.clients': '1 200+',
      'portal.stats.uptime': '99.9%',
      'portal.stats.regions': '3',
    }
    vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => settings[key] ?? '' }))
    const wrapper = mountWithKit(AuroraHero)

    expect(wrapper.find('[data-testid="hero-badge"]').text()).toContain('Fast and calm')
    expect(wrapper.find('[data-testid="hero-stats"]').text()).toContain('1 200+')
    expect(wrapper.find('[data-testid="hero-stats"]').text()).toContain('99.9%')
    expect(wrapper.find('[data-testid="hero-stats"]').text()).toContain('aurora.hero.statRegions')
  })

  it('leaves out an unset figure and hides the badge and the figures row when nothing is set', () => {
    stubAuroraGlobals()
    vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => (key === 'portal.stats.uptime' ? '99.9%' : '') }))
    const partial = mountWithKit(AuroraHero)
    expect(partial.find('[data-testid="hero-stats"]').text()).toContain('99.9%')
    expect(partial.find('[data-testid="hero-stats"]').text()).not.toContain('aurora.hero.statClients')

    vi.stubGlobal('usePortalSettings', () => ({ get: () => '' }))
    const empty = mountWithKit(AuroraHero)
    expect(empty.find('[data-testid="hero-badge"]').exists()).toBe(false)
    expect(empty.find('[data-testid="hero-stats"]').exists()).toBe(false)
  })

  it('paints the primary button with the brand gradient and the brand tint text', () => {
    stubAuroraGlobals()
    const classes = mountWithKit(AuroraHero).find('[data-testid="hero-cta-primary"]').classes()

    expect(classes).toEqual(expect.arrayContaining(['bg-brand', 'text-on-brand']))
  })
})

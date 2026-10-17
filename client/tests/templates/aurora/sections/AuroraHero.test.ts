/**
 * Unit tests for AuroraHero: the single page headline, both calls to action and the headline figures.
 */
import { describe, expect, it } from 'vitest'
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

  it('shows the three headline figures with their captions', () => {
    stubAuroraGlobals()
    const text = mountWithKit(AuroraHero).text()

    expect(text).toContain('24/7')
    expect(text).toContain('aurora.hero.statDomains')
    expect(text).toContain('aurora.hero.statProjects')
    expect(text).toContain('aurora.hero.statSupport')
  })

  it('paints the primary button with the brand gradient and the brand tint text', () => {
    stubAuroraGlobals()
    const classes = mountWithKit(AuroraHero).find('[data-testid="hero-cta-primary"]').classes()

    expect(classes).toEqual(expect.arrayContaining(['bg-brand', 'text-on-tint']))
  })
})

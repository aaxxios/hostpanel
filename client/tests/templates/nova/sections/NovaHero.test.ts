/**
 * Unit tests for NovaHero: the page's only h1, its actions and the gated benefit list.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaHero from '~/templates/nova/sections/NovaHero.vue'
import { HERO_BENEFITS } from '~/templates/nova/content'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'

describe('NovaHero', () => {
  it('owns the single h1 and links both actions', () => {
    stubTemplateGlobals()
    const wrapper = mountWithKit(NovaHero)

    expect(wrapper.findAll('h1')).toHaveLength(1)
    expect(wrapper.find('h1').text()).toBe('nova.hero.title')
    expect(wrapper.find('[data-testid="nova-hero-cta-primary"]').attributes('href')).toBe('/hosting')
    expect(wrapper.find('[data-testid="nova-hero-cta-secondary"]').attributes('href')).toBe('/hosting#plans')
  })

  it('hides a benefit whose operator setting is empty', () => {
    stubTemplateGlobals()
    const wrapper = mountWithKit(NovaHero)
    const gated = HERO_BENEFITS.filter(benefit => benefit.requiresSetting).length

    expect(wrapper.find('ul').findAll('li')).toHaveLength(HERO_BENEFITS.length - gated)
  })

  it('shows every benefit once the settings are filled in', () => {
    stubTemplateGlobals({ settings: { 'portal.features.backups': 'true' } })
    const wrapper = mountWithKit(NovaHero)

    expect(wrapper.find('ul').findAll('li')).toHaveLength(HERO_BENEFITS.length)
  })
})

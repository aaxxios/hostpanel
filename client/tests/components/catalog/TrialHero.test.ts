/**
 * Unit tests for TrialHero: the title, the trial and learn-more calls to action and the accent colour.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import TrialHero from '~/components/catalog/TrialHero.vue'

/** Mounts the hero. */
const mountHero = () =>
  mountWithKit(TrialHero, {
    props: { name: 'Taskero', icon: 'view-dashboard', color: '#10b981', trialUrl: '/products/taskero', learnMoreUrl: '/products/taskero' },
  })

describe('TrialHero', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
  })

  it('shows the title and the subtitle', () => {
    const wrapper = mountHero()

    expect(wrapper.find('[data-testid="trial-title"]').element.tagName).toBe('H1')
    expect(wrapper.text()).toContain('trial.subtitle')
    expect(wrapper.text()).toContain('trial.noCc')
  })

  it('opens the trial in a new tab without giving the new page access to this one', () => {
    const cta = mountHero().find('[data-testid="trial-cta"]')

    expect(cta.attributes('href')).toBe('/products/taskero')
    expect(cta.attributes('target')).toBe('_blank')
    expect(cta.attributes('rel')).toBe('noopener noreferrer')
  })

  it('links learn more to the localised catalogue page', () => {
    expect(mountHero().find('[data-testid="trial-learn-more"]').attributes('href')).toBe('/en/products/taskero')
  })

  it('paints the hero in the product accent colour', () => {
    const accent = mountHero().find('[data-testid="ui-accent"]').element as HTMLElement

    expect(accent.style.getPropertyValue('--accent')).toBe('#10b981')
  })
})

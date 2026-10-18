/**
 * Unit tests for SectionsHero: the headline, the calls to action, the counters that run up on load
 * and on hover, and the scroll hint.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import SectionsHero from '~/components/sections/SectionsHero.vue'
import { teamStats } from '~/lib/data'

describe('SectionsHero', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('shows the headline as the page heading, with the badge and subtitle', () => {
    const wrapper = mountWithKit(SectionsHero)

    expect(wrapper.find('h1').exists()).toBe(true)
    expect(wrapper.find('h1').text()).toContain('hero.title')
    expect(wrapper.text()).toContain('hero.badge')
    expect(wrapper.text()).toContain('hero.subtitle')
  })

  it('links the two calls to action to the contact page and the hosting plans', () => {
    const wrapper = mountWithKit(SectionsHero)

    expect(wrapper.find('[data-testid="hero-primary"]').attributes('href')).toBe('/en/contact')
    expect(wrapper.find('[data-testid="hero-secondary"]').attributes('href')).toBe('/en/hosting')
  })

  it('starts every counter at zero and runs them up to the company figures after a moment', async () => {
    const wrapper = mountWithKit(SectionsHero)
    expect(wrapper.find('[data-testid="hero-stat-0"]').text()).toContain('0+')

    await vi.advanceTimersByTimeAsync(800 + 2500)

    expect(wrapper.find('[data-testid="hero-stat-0"]').text()).toContain(`${teamStats.projects}+`)
    expect(wrapper.find('[data-testid="hero-stat-1"]').text()).toContain(`${teamStats.years}`)
    expect(wrapper.find('[data-testid="hero-stat-2"]').text()).toContain(`${teamStats.teamSize}+`)
    expect(wrapper.find('[data-testid="hero-stat-3"]').text()).toContain(`${teamStats.clients}+`)
  })

  it('runs a counter up from hover without waiting for the load animation', async () => {
    const wrapper = mountWithKit(SectionsHero)

    await wrapper.find('[data-testid="hero-stat-0"]').trigger('mouseenter')
    await vi.advanceTimersByTimeAsync(2000)

    expect(wrapper.find('[data-testid="hero-stat-0"]').text()).toContain(`${teamStats.projects}+`)
  })

  it('scrolls down by one screen from the scroll hint', async () => {
    const scrollTo = vi.fn()
    window.scrollTo = scrollTo as unknown as typeof window.scrollTo
    const wrapper = mountWithKit(SectionsHero)

    await wrapper.find('[data-testid="hero-scroll"]').trigger('click')

    expect(scrollTo).toHaveBeenCalledWith({ top: window.innerHeight, behavior: 'smooth' })
  })

  it('stops the counters when it is removed', async () => {
    const clear = vi.spyOn(globalThis, 'clearInterval')
    const wrapper = mountWithKit(SectionsHero)
    await wrapper.find('[data-testid="hero-stat-0"]').trigger('mouseenter')

    wrapper.unmount()

    expect(clear).toHaveBeenCalled()
  })
})

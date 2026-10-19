/**
 * Unit tests for SectionsHero: the headline, the calls to action, the operator-set figures (hidden
 * while empty, counted up on load and on hover when they are plain numbers) and the scroll hint.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { nextTick, reactive } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import SectionsHero from '~/components/sections/SectionsHero.vue'
import { PortalSettingKey } from '~/constants/portalSettingKey'

/**
 * Stands in for the operator settings the hero reads.
 *
 * @param settings Setting values by key; a key that is absent reads as empty.
 */
const stubSettings = (settings: Record<string, string>): void => {
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => settings[key] ?? '' }))
}

/** All three figures set: a counted one with a sign, a decimal one and a plain number. */
const ALL_FIGURES: Record<string, string> = {
  [PortalSettingKey.StatsClients]: '150+',
  [PortalSettingKey.StatsUptime]: '99.9%',
  [PortalSettingKey.StatsRegions]: '3',
}

describe('SectionsHero', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
    stubSettings(ALL_FIGURES)
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

  it('captions each configured figure with its own label', () => {
    const wrapper = mountWithKit(SectionsHero)

    expect(wrapper.find('[data-testid="hero-stat-0"]').text()).toContain('hero.stats.clients')
    expect(wrapper.find('[data-testid="hero-stat-1"]').text()).toContain('hero.stats.uptime')
    expect(wrapper.find('[data-testid="hero-stat-2"]').text()).toContain('hero.stats.regions')
  })

  it('starts a countable figure at zero and runs it up to the configured value after a moment', async () => {
    const wrapper = mountWithKit(SectionsHero)
    expect(wrapper.find('[data-testid="hero-stat-0"]').text()).toContain('0+')

    await vi.advanceTimersByTimeAsync(800 + 2500)

    expect(wrapper.find('[data-testid="hero-stat-0"]').text()).toContain('150+')
    expect(wrapper.find('[data-testid="hero-stat-2"]').text()).toContain('3')
  })

  it('shows a figure that is not a plain number exactly as the operator wrote it', async () => {
    const wrapper = mountWithKit(SectionsHero)
    expect(wrapper.find('[data-testid="hero-stat-1"]').text()).toContain('99.9%')

    await vi.advanceTimersByTimeAsync(800 + 2500)

    expect(wrapper.find('[data-testid="hero-stat-1"]').text()).toContain('99.9%')
  })

  it('leaves out a figure whose setting is empty and hides the block when none is set', () => {
    stubSettings({ [PortalSettingKey.StatsUptime]: '99.9%' })
    const partial = mountWithKit(SectionsHero)

    expect(partial.findAll('[data-testid^="hero-stat-"]')).toHaveLength(1)
    expect(partial.find('[data-testid="hero-stat-0"]').text()).toContain('hero.stats.uptime')
    expect(partial.text()).not.toContain('hero.stats.clients')

    stubSettings({})
    const none = mountWithKit(SectionsHero)

    expect(none.find('[data-testid="hero-stats"]').exists()).toBe(false)
  })

  it('runs a counter up from hover without waiting for the load animation', async () => {
    const wrapper = mountWithKit(SectionsHero)

    await wrapper.find('[data-testid="hero-stat-0"]').trigger('mouseenter')
    await vi.advanceTimersByTimeAsync(2000)

    expect(wrapper.find('[data-testid="hero-stat-0"]').text()).toContain('150+')
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

  it('re-initialises the counters when the figures arrive after the first render', async () => {
    const settings = reactive<Record<string, string>>({})
    vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => settings[key] ?? '' }))
    const wrapper = mountWithKit(SectionsHero)
    expect(wrapper.find('[data-testid="hero-stats"]').exists()).toBe(false)

    Object.assign(settings, ALL_FIGURES)
    await nextTick()
    expect(wrapper.find('[data-testid="hero-stat-0"]').text()).not.toContain('undefined')

    await vi.advanceTimersByTimeAsync(800 + 2500)
    expect(wrapper.find('[data-testid="hero-stat-0"]').text()).toContain('150+')
    expect(wrapper.find('[data-testid="hero-stat-0"]').text()).not.toContain('undefined')
  })
})

/**
 * Unit tests for CookieBanner: it shows only until a choice exists, and each button records the
 * matching choice and hides the banner.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { nextTick, ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import CookieBanner from '~/components/ui/UiCookieBanner.vue'
import { CookieConsent } from '~/types/cookieconsent'
import { COOKIE_BANNER_HEIGHT_VAR } from '~/utils/ui/cookieBanner'

/** The consent state the stubbed composable exposes. */
const consent = ref<CookieConsent | null>(null)

/** Spy for the composable's accept action. */
const accept = vi.fn()

describe('CookieBanner', () => {
  beforeEach(() => {
    consent.value = null
    accept.mockClear()
    vi.stubGlobal('useCookieConsent', () => ({ consent, accept }))
    vi.stubGlobal('useLocalePath', () => (path: string) => '/en' + path)
  })

  it('shows after mount when no choice was made', async () => {
    const wrapper = mountWithKit(CookieBanner)
    await nextTick()

    expect(wrapper.find('[data-testid="cookie-banner"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('cookieBanner.title')
  })

  it('publishes its measured height for the phone spacer and the contact button, and clears it on dismissal', async () => {
    const wrapper = mountWithKit(CookieBanner)
    await nextTick()

    expect(document.documentElement.style.getPropertyValue(COOKIE_BANNER_HEIGHT_VAR)).toMatch(/^\d+(\.\d+)?px$/)
    expect(wrapper.find('[data-testid="cookie-banner-spacer"]').classes()).toContain(`h-[var(${COOKIE_BANNER_HEIGHT_VAR},0px)]`)

    await wrapper.find('[data-testid="cookie-banner-accept-all"]').trigger('click')
    await nextTick()

    expect(document.documentElement.style.getPropertyValue(COOKIE_BANNER_HEIGHT_VAR)).toBe('')
  })

  it('floats as a compact rounded card instead of a full-width bar', async () => {
    const wrapper = mountWithKit(CookieBanner)
    await nextTick()

    const banner = wrapper.find('[data-testid="cookie-banner"]')
    expect(banner.classes()).toEqual(expect.arrayContaining(['rounded-2xl', 'sm:w-[400px]', 'border']))
    expect(banner.classes()).not.toContain('bottom-0')
  })

  it('stays hidden when a choice already exists', async () => {
    consent.value = CookieConsent.All

    const wrapper = mountWithKit(CookieBanner)
    await nextTick()

    expect(wrapper.find('[data-testid="cookie-banner"]').exists()).toBe(false)
  })

  it('links to the cookie policy in the current language', async () => {
    const wrapper = mountWithKit(CookieBanner)
    await nextTick()

    expect(wrapper.find('[data-testid="cookie-banner-learn-more"]').attributes('href')).toBe('/en/cookie-policy')
  })

  it('records essential-only and hides', async () => {
    const wrapper = mountWithKit(CookieBanner)
    await nextTick()

    await wrapper.find('[data-testid="cookie-banner-essential"]').trigger('click')

    expect(accept).toHaveBeenCalledWith(CookieConsent.Essential)
    expect(wrapper.find('[data-testid="cookie-banner"]').exists()).toBe(false)
  })

  it('records accept-all and hides', async () => {
    const wrapper = mountWithKit(CookieBanner)
    await nextTick()

    await wrapper.find('[data-testid="cookie-banner-accept-all"]').trigger('click')

    expect(accept).toHaveBeenCalledWith(CookieConsent.All)
    expect(wrapper.find('[data-testid="cookie-banner"]').exists()).toBe(false)
  })

  it('paints its dark surface with the plain token, never an opacity modifier', async () => {
    const wrapper = mountWithKit(CookieBanner)
    await nextTick()

    // `bg-ui-surface` holds a var() colour; Tailwind cannot apply `/95` to it, so the class is
    // never generated and the banner stayed white in dark mode with white text on it.
    const classes = wrapper.find('[data-testid="cookie-banner"]').classes()
    expect(classes).toContain('dark:bg-ui-surface')
    expect(classes.some((c) => c.startsWith('dark:bg-ui-surface/'))).toBe(false)
  })

  it('reserves room in the page flow on a phone only, so no blank band is left under the footer from sm up', async () => {
    const wrapper = mountWithKit(CookieBanner)
    await nextTick()

    // The full-width phone card covers the end of the page, so the spacer is there; the corner card
    // from `sm` floats over the page and its spacer is hidden.
    expect(wrapper.find('[data-testid="cookie-banner"]').classes()).toContain('fixed')
    const spacer = wrapper.find('[data-testid="cookie-banner-spacer"]')
    expect(spacer.exists()).toBe(true)
    expect(spacer.classes()).toContain('sm:hidden')

    await wrapper.find('[data-testid="cookie-banner-accept-all"]').trigger('click')
    await nextTick()

    expect(wrapper.find('[data-testid="cookie-banner-spacer"]').exists()).toBe(false)
  })
})

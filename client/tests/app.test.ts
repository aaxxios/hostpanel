/**
 * Tests for the root app component: the layout, the page slot and the cookie banner it renders,
 * the global structured data it injects, the document language, the site identity metas and the
 * favicon links it hands to the head.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { defineComponent, h, ref, unref } from 'vue'
import type { Ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import App from '~/app.vue'
import { toHeadIconLink } from '~/utils/brandingIcons'

/** Spy for `useHead`, to read the language and the icon links the app puts on the document. */
const useHead = vi.fn()

/** Spy for `useSeoMeta`, to read the identity metas the app sets. */
const useSeoMeta = vi.fn()

/** Spy for `injectSchema`, to read the structured data the app injects. */
const injectSchema = vi.fn()

/** Spy for `brandingIcons`, to read the favicon the links are built from. */
const brandingIcons = vi.fn()

/** The page the root's `NuxtPage` shows. */
const PAGE_TESTID = 'page'

/**
 * Builds a component that renders its default slot inside a marked element.
 *
 * @param testid - The `data-testid` the element carries.
 * @returns The component.
 */
const slotMarker = (testid: string) => defineComponent({
  setup: (_props, { slots }) => () => h('div', { 'data-testid': testid }, slots.default?.()),
})

/**
 * Builds a component that renders a marked, empty element.
 *
 * @param testid - The `data-testid` the element carries.
 * @returns The component.
 */
const marker = (testid: string) => defineComponent({ setup: () => () => h('div', { 'data-testid': testid }) })

/**
 * Stubs the composables the app reads.
 *
 * @param options - What the stubs answer.
 * @param options.locale - The active UI locale.
 * @param options.tagline - The operator's tagline; empty means none.
 * @param options.favicon - The `portal.favicon` setting.
 * @returns The locale ref and the site name ref, so a test can read them back.
 */
const stubComposables = (options: { locale?: string, tagline?: string, favicon?: string } = {}): { locale: Ref<string>, siteName: Ref<string> } => {
  /** The active UI locale. */
  const locale = ref(options.locale ?? 'en')
  /** The operator-set site name. */
  const siteName = ref('Acme Hosting')
  vi.stubGlobal('useI18n', () => ({ t: (key: string) => key, locale }))
  vi.stubGlobal('useSchemaOrg', () => ({
    organizationSchema: () => ({ kind: 'organization' }),
    localBusinessSchema: () => ({ kind: 'localBusiness' }),
    websiteSchema: () => ({ kind: 'website' }),
    injectSchema,
  }))
  vi.stubGlobal('usePortalSettings', () => ({ get: () => options.favicon ?? '' }))
  vi.stubGlobal('useSiteIdentity', () => ({ name: siteName, tagline: ref(options.tagline ?? '') }))
  vi.stubGlobal('useHead', useHead)
  vi.stubGlobal('useSeoMeta', useSeoMeta)
  vi.stubGlobal('brandingIcons', brandingIcons)
  // The real mapper, not a spy: the expected links below are what it produces.
  vi.stubGlobal('toHeadIconLink', toHeadIconLink)
  return { locale, siteName }
}

/**
 * Mounts the app with slot-rendering stand-ins for the layout and the page.
 *
 * @returns The wrapper.
 */
const mountApp = () => mountWithKit(App, {
  components: {
    NuxtLayout: slotMarker('layout'),
    NuxtPage: marker(PAGE_TESTID),
  },
  stubs: { UiCookieBanner: marker('cookie-banner') },
})

/**
 * Reads the object the app passed to `useHead`.
 *
 * @returns The head input.
 */
const headInput = () => useHead.mock.calls[0]![0] as {
  htmlAttrs: { lang: () => string }
  link: Ref<Array<Record<string, string>>>
}

/**
 * Reads the object the app passed to `useSeoMeta`.
 *
 * @returns The identity metas, each a plain value or a ref.
 */
const seoMeta = () => useSeoMeta.mock.calls[0]![0] as Record<string, unknown>

describe('app.vue', () => {
  beforeEach(() => {
    useHead.mockReset()
    useSeoMeta.mockReset()
    injectSchema.mockReset()
    brandingIcons.mockReset()
    brandingIcons.mockReturnValue([])
  })

  it('renders the layout around the page and leaves the cookie banner to the layouts', () => {
    stubComposables()

    const wrapper = mountApp()

    expect(wrapper.find('[data-testid="layout"] [data-testid="page"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="cookie-banner"]').exists()).toBe(false)
  })

  it('injects the organization, local business and website schemas once', () => {
    stubComposables()

    mountApp()

    expect(injectSchema).toHaveBeenCalledTimes(1)
    expect(injectSchema).toHaveBeenCalledWith([
      { kind: 'organization' },
      { kind: 'localBusiness' },
      { kind: 'website' },
    ])
  })

  it.each(['en', 'ru', 'hy'])('sets the document language to %s', (code) => {
    stubComposables({ locale: code })

    mountApp()

    expect(headInput().htmlAttrs.lang()).toBe(code)
  })

  it('falls back to the default language for an unknown locale', () => {
    stubComposables({ locale: 'xx' })

    mountApp()

    expect(headInput().htmlAttrs.lang()).toBe('en')
  })

  it('follows the locale when it changes', () => {
    const { locale } = stubComposables({ locale: 'en' })

    mountApp()
    locale.value = 'hy'

    expect(headInput().htmlAttrs.lang()).toBe('hy')
  })

  it('sets the site name as og:site_name and author', () => {
    const { siteName } = stubComposables()

    mountApp()

    const meta = seoMeta()
    expect(unref(meta.ogSiteName)).toBe(siteName.value)
    expect(unref(meta.author)).toBe(siteName.value)
  })

  it('leaves the description alone when there is no tagline', () => {
    stubComposables({ tagline: '' })

    mountApp()

    const meta = seoMeta()
    expect(meta).not.toHaveProperty('description')
    expect(meta).not.toHaveProperty('ogDescription')
  })

  it('sets the description and og:description from the tagline', () => {
    stubComposables({ tagline: 'Hosting that stays up' })

    mountApp()

    const meta = seoMeta()
    expect(unref(meta.description)).toBe('Hosting that stays up')
    expect(unref(meta.ogDescription)).toBe('Hosting that stays up')
  })

  it('builds the favicon links from the favicon setting', () => {
    stubComposables({ favicon: '/uploads/branding/abc/favicon.png' })

    mountApp()
    void headInput().link.value

    expect(brandingIcons).toHaveBeenCalledWith('/uploads/branding/abc/favicon.png')
  })

  it('passes type and sizes only for the icons that have them', () => {
    brandingIcons.mockReturnValue([
      { rel: 'icon', href: '/favicon.ico', type: 'image/x-icon' },
      { rel: 'icon', href: '/favicon-32x32.png', type: 'image/png', sizes: '32x32' },
      { rel: 'icon', href: '/bare.png', type: '', sizes: '' },
    ])
    stubComposables()

    mountApp()

    expect(headInput().link.value).toEqual([
      { rel: 'icon', href: '/favicon.ico', type: 'image/x-icon' },
      { rel: 'icon', href: '/favicon-32x32.png', type: 'image/png', sizes: '32x32' },
      { rel: 'icon', href: '/bare.png' },
    ])
  })
})

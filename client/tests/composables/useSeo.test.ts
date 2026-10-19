/**
 * Tests for `useSeo`: the canonical URL, share image, Open Graph locale and head links a page
 * gets from the options it passes.
 *
 * @module tests/composables/useSeo.test
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { SeoPageType } from '~/types/seopagetype'
import { useSeo } from '~/composables/useSeo'
import type { SeoOptions } from '~/composables/useSeo'

/** The base URL of the stood-in site. */
const BASE = 'https://host.test'

/** Spy for `useSeoMeta`, to read the tags the composable set. */
const useSeoMeta = vi.fn()

/** Spy for `useHead`, to read the links the composable set. */
const useHead = vi.fn()

/**
 * Stubs the Nuxt globals `useSeo` reads and records the head calls.
 *
 * @param locale The active language.
 * @param routePath The path of the current route.
 */
const stubRuntime = (locale = 'en', routePath = '/hosting'): void => {
  vi.stubGlobal('useI18n', () => ({ locale: ref(locale) }))
  vi.stubGlobal('useRuntimeConfig', () => ({ public: { baseUrl: BASE } }))
  vi.stubGlobal('useRoute', () => ({ path: routePath }))
  vi.stubGlobal('useSeoMeta', useSeoMeta)
  vi.stubGlobal('useHead', useHead)
  vi.stubGlobal('useSiteIdentity', () => ({ name: ref('Site') }))
}

/**
 * Reads the first argument of the single `useSeoMeta` call.
 *
 * @returns The meta tags that were set.
 */
const metaSet = (): Record<string, unknown> => useSeoMeta.mock.calls[0]![0] as Record<string, unknown>

/**
 * Reads the first argument of the single `useHead` call.
 *
 * @returns The head input the composable set.
 */
const headSet = (): { meta: unknown[] } => useHead.mock.calls[0]![0] as { meta: unknown[] }

describe('useSeo', () => {
  beforeEach(() => {
    useSeoMeta.mockClear()
    useHead.mockClear()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('builds the canonical URL from the route path in English', () => {
    stubRuntime('en', '/hosting')

    const { canonicalUrl, baseUrl } = useSeo({})

    expect(baseUrl).toBe(BASE)
    expect(canonicalUrl.value).toBe(`${BASE}/hosting`)
  })

  it('prefixes the canonical URL with the language outside English and strips a prefix already on the path', () => {
    stubRuntime('ru', '/ru/hosting')

    expect(useSeo({}).canonicalUrl.value).toBe(`${BASE}/ru/hosting`)
  })

  it('prefers the path the page passes over the route path', () => {
    stubRuntime('en', '/hosting')

    expect(useSeo({ path: '/domains' }).canonicalUrl.value).toBe(`${BASE}/domains`)
  })

  it('sets the title, description and keywords for every card', () => {
    stubRuntime()
    const options: SeoOptions = { title: 'T', description: 'D', keywords: 'k' }

    useSeo(options)

    expect(metaSet()).toMatchObject({
      title: 'T',
      description: 'D',
      ogTitle: 'T',
      ogDescription: 'D',
      twitterTitle: 'T',
      twitterDescription: 'D',
      twitterCard: 'summary_large_image',
    })
    expect(headSet().meta).toEqual([{ name: 'keywords', content: 'k' }])
  })

  it('adds no keywords tag when the page gives none', () => {
    stubRuntime()

    useSeo({ title: 'T' })

    expect(headSet().meta).toEqual([])
  })

  it('uses the default share image unless the page supplies one', () => {
    stubRuntime()
    useSeo({})
    expect(metaSet()).toMatchObject({
      ogImage: `${BASE}/og-image.jpg`,
      twitterImage: `${BASE}/og-image.jpg`,
      ogImageWidth: 1200,
      ogImageHeight: 630,
      ogImageType: 'image/jpeg',
      ogImageAlt: 'Site',
    })

    useSeoMeta.mockClear()
    useSeo({ image: '/img/x.png' })
    expect(metaSet()).toMatchObject({ ogImage: `${BASE}/img/x.png`, twitterImage: `${BASE}/img/x.png` })
  })

  it('defaults the Open Graph type to website and honours an explicit one', () => {
    stubRuntime()
    useSeo({})
    expect(metaSet().ogType).toBe(SeoPageType.Website)

    useSeoMeta.mockClear()
    useSeo({ type: SeoPageType.Article })
    expect(metaSet().ogType).toBe(SeoPageType.Article)
  })

  it.each([
    ['en', 'en_US'],
    ['ru', 'ru_RU'],
    ['hy', 'hy_AM'],
    ['de', 'en_US'],
  ])('maps the %s language to the %s Open Graph locale', (locale, expected) => {
    stubRuntime(locale)

    useSeo({})

    expect(metaSet().ogLocale).toBe(expected)
  })

  it('adds the canonical link and one alternate per language to the head', () => {
    stubRuntime('en', '/hosting')

    useSeo({})

    const links = (useHead.mock.calls[0]![0] as { link: Array<{ rel: string, hreflang?: string, href: string }> }).link
    expect(links[0]).toEqual({ rel: 'canonical', href: `${BASE}/hosting` })
    expect(links.filter((link) => link.rel === 'alternate').map((link) => link.hreflang)).toEqual(
      expect.arrayContaining(['en', 'ru', 'hy', 'x-default']),
    )
  })
})

/**
 * Tests for `useSchemaOrg`: the JSON-LD documents the public site publishes and their injection
 * into the document head.
 *
 * @module tests/composables/useSchemaOrg.test
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { useSchemaOrg } from '~/composables/useSchemaOrg'

/** The base URL of the stood-in site. */
const BASE = 'https://host.test'

/** Spy for `useHead`, to read the scripts that were injected. */
const useHead = vi.fn()

/**
 * Stubs the Nuxt globals `useSchemaOrg` reads.
 *
 * @param settings Operator setting key to value.
 * @param baseUrl The configured public base URL; empty selects the built-in fallback.
 * @param tagline The operator's tagline.
 */
const stubRuntime = (settings: Record<string, string> = {}, baseUrl = BASE, tagline = ''): void => {
  vi.stubGlobal('useRuntimeConfig', () => ({ public: { baseUrl } }))
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => settings[key] ?? '' }))
  vi.stubGlobal('useSiteIdentity', () => ({ name: ref('Acme Hosting'), tagline: ref(tagline) }))
  vi.stubGlobal('useHead', useHead)
}

describe('useSchemaOrg', () => {
  beforeEach(() => {
    useHead.mockClear()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('exposes the configured base URL and falls back when none is set', () => {
    stubRuntime()
    expect(useSchemaOrg().baseUrl).toBe(BASE)

    stubRuntime({}, '')
    expect(useSchemaOrg().baseUrl).toBe('https://yourdomain.com')
  })

  it('names the organisation after the operator and uses their tagline when set', () => {
    stubRuntime({}, BASE, 'Calm hosting')

    const schema = useSchemaOrg().organizationSchema()

    expect(schema['@type']).toBe('Organization')
    expect(schema.name).toBe('Acme Hosting')
    expect(schema.description).toBe('Calm hosting')
    expect(schema['@id']).toBe(`${BASE}/#organization`)
  })

  it('builds sameAs from the operator contact channels and omits unset ones', () => {
    stubRuntime({ 'portal.contact.telegram': 'acme', 'portal.contact.whatsapp': '' })

    expect(useSchemaOrg().organizationSchema().sameAs).toEqual(['https://t.me/acme'])
  })

  it('links the website schema to the organisation and lists every language', () => {
    stubRuntime()

    const schema = useSchemaOrg().websiteSchema()

    expect(schema['@type']).toBe('WebSite')
    expect(schema.publisher['@id']).toBe(`${BASE}/#organization`)
    expect(schema.inLanguage).toEqual(['en', 'ru', 'hy'])
  })

  it('names the local business after the operator', () => {
    stubRuntime()

    const schema = useSchemaOrg().localBusinessSchema()

    expect(schema['@type']).toBe('LocalBusiness')
    expect(schema.name).toBe('Acme Hosting')
  })

  it('makes a relative service URL absolute and keeps an absolute one', () => {
    stubRuntime()
    const { serviceSchema } = useSchemaOrg()
    const service = { name: 'S', description: 'D', serviceType: 'Hosting' }

    expect(serviceSchema({ ...service, url: '/services/x' }).url).toBe(`${BASE}/services/x`)
    expect(serviceSchema({ ...service, url: 'https://other.test/x' }).url).toBe('https://other.test/x')
  })

  it('makes a relative product URL absolute and keeps an absolute one', () => {
    stubRuntime()
    const { productSchema } = useSchemaOrg()
    const product = { name: 'P', description: 'D' }

    expect(productSchema({ ...product, url: '/p' }).url).toBe(`${BASE}/p`)
    expect(productSchema({ ...product, url: 'https://other.test/p' }).url).toBe('https://other.test/p')
  })

  it('numbers breadcrumb steps from one and absolutises their URLs', () => {
    stubRuntime()

    const schema = useSchemaOrg().breadcrumbSchema([
      { name: 'Home', url: '/' },
      { name: 'Hosting', url: 'https://other.test/h' },
    ])

    expect(schema.itemListElement).toEqual([
      { '@type': 'ListItem', position: 1, name: 'Home', item: `${BASE}/` },
      { '@type': 'ListItem', position: 2, name: 'Hosting', item: 'https://other.test/h' },
    ])
  })

  it('maps FAQ entries to questions with accepted answers', () => {
    stubRuntime()

    const schema = useSchemaOrg().faqSchema([{ question: 'Q?', answer: 'A.' }])

    expect(schema['@type']).toBe('FAQPage')
    expect(schema.mainEntity).toEqual([
      { '@type': 'Question', name: 'Q?', acceptedAnswer: { '@type': 'Answer', text: 'A.' } },
    ])
  })

  it('injects one JSON-LD script per schema into the head', () => {
    stubRuntime()
    const { injectSchema } = useSchemaOrg()
    const first = { '@type': 'A' }
    const second = { '@type': 'B' }

    injectSchema([first, second])

    const scripts = (useHead.mock.calls[0]![0] as { script: Array<{ type: string, innerHTML: string }> }).script
    expect(scripts).toEqual([
      { type: 'application/ld+json', innerHTML: JSON.stringify(first) },
      { type: 'application/ld+json', innerHTML: JSON.stringify(second) },
    ])
  })

  it('accepts a single schema object as well as a list', () => {
    stubRuntime()

    useSchemaOrg().injectSchema({ '@type': 'A' })

    expect((useHead.mock.calls[0]![0] as { script: unknown[] }).script).toHaveLength(1)
  })
})

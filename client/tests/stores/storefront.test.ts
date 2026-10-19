/**
 * Tests for `stores/storefront.ts`.
 *
 * The store, not the sections, owns the lists, the loading flag and the failure sentence, so these
 * assertions pin the contract the sections render from -- including that an empty list is a valid
 * state and a refusal leaves an empty list rather than stale data.
 *
 * @module tests/stores/storefront.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { faqFixture, partnerFixture, projectFixture, testimonialFixture } from '../helpers/storefrontFixtures'
import { FaqCategory } from '~/types/faqcategory'
import { StorefrontSection } from '~/types/storefrontsection'

/** The mocked storefront endpoints, re-created for every test. */
const api = {
  fetchTestimonials: vi.fn(),
  fetchPartners: vi.fn(),
  fetchProjects: vi.fn(),
  fetchFaqs: vi.fn()
}

vi.mock('~/composables/apis/useStorefrontApi', () => ({
  /**
   * Stands in for the real API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useStorefrontApi: () => api
}))

const { useStorefrontStore } = await import('~/stores/storefront')

/** An API refusal as `apiErrorMessage` reads it. */
const refusal = { data: { detail: 'Content unavailable.' } }

describe('useStorefrontStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    Object.values(api).forEach(fn => fn.mockReset())
  })

  it('starts with nothing, which is a valid state', () => {
    const store = useStorefrontStore()

    expect(store.testimonials).toEqual([])
    expect(store.partners).toEqual([])
    expect(store.projects).toEqual([])
    expect(store.faqs).toEqual([])
    expect(store.loading).toBe(false)
    expect(Object.values(store.errors).every(sentence => sentence === '')).toBe(true)
  })

  it('keeps the testimonials', async () => {
    api.fetchTestimonials.mockResolvedValue([testimonialFixture(1)])
    const store = useStorefrontStore()

    const list = await store.loadTestimonials()

    expect(list).toHaveLength(1)
    expect(store.testimonials[0]?.name).toBe('Example Person 1')
  })

  it('keeps the partners', async () => {
    api.fetchPartners.mockResolvedValue([partnerFixture(1), partnerFixture(2)])
    const store = useStorefrontStore()

    await store.loadPartners()

    expect(store.partners.map(p => p.id)).toEqual([1, 2])
  })

  it('keeps the projects', async () => {
    api.fetchProjects.mockResolvedValue([projectFixture(1)])
    const store = useStorefrontStore()

    await store.loadProjects()

    expect(store.projects[0]?.slug).toBe('example-project-1')
  })

  it('keeps the questions and picks those of one topic', async () => {
    api.fetchFaqs.mockResolvedValue([
      faqFixture(1),
      faqFixture(2, { category: FaqCategory.Products }),
      faqFixture(3, { category: FaqCategory.Products })
    ])
    const store = useStorefrontStore()

    await store.loadFaqs()

    expect(store.faqs).toHaveLength(3)
    expect(store.faqsIn(FaqCategory.Products).map(f => f.id)).toEqual([2, 3])
    expect(store.faqsIn(FaqCategory.General).map(f => f.id)).toEqual([1])
  })

  it.each([
    ['loadTestimonials', 'fetchTestimonials', 'testimonials', StorefrontSection.Testimonials],
    ['loadPartners', 'fetchPartners', 'partners', StorefrontSection.Partners],
    ['loadProjects', 'fetchProjects', 'projects', StorefrontSection.Projects],
    ['loadFaqs', 'fetchFaqs', 'faqs', StorefrontSection.Faqs]
  ] as const)('%s keeps the API sentence and an empty list when refused', async (load, fetcher, state, section) => {
    api[fetcher].mockRejectedValue(refusal)
    const store = useStorefrontStore()

    const list = await store[load]()

    expect(list).toEqual([])
    expect(store[state]).toEqual([])
    expect(store.errors[section]).toBe('Content unavailable.')
    expect(store.loading).toBe(false)
  })

  it('drops stale rows when a later read is refused', async () => {
    api.fetchPartners.mockResolvedValueOnce([partnerFixture(1)]).mockRejectedValueOnce(refusal)
    const store = useStorefrontStore()

    await store.loadPartners()
    await store.loadPartners()

    expect(store.partners).toEqual([])
  })

  it('flags loading while a read is in flight', async () => {
    api.fetchFaqs.mockResolvedValue([])
    const store = useStorefrontStore()

    const pending = store.loadFaqs()
    expect(store.loading).toBe(true)
    await pending

    expect(store.loading).toBe(false)
  })

  it('clears the failure sentence when the next read succeeds', async () => {
    api.fetchFaqs.mockRejectedValueOnce(refusal).mockResolvedValueOnce([faqFixture(1)])
    const store = useStorefrontStore()

    await store.loadFaqs()
    expect(store.errors[StorefrontSection.Faqs]).toBe('Content unavailable.')
    await store.loadFaqs()

    expect(store.errors[StorefrontSection.Faqs]).toBe('')
  })

  it('keeps the failure of one section when another section loads successfully', async () => {
    api.fetchFaqs.mockRejectedValue(refusal)
    api.fetchPartners.mockResolvedValue([partnerFixture(1)])
    const store = useStorefrontStore()

    await Promise.all([store.loadFaqs(), store.loadPartners()])

    expect(store.errors[StorefrontSection.Faqs]).toBe('Content unavailable.')
    expect(store.errors[StorefrontSection.Partners]).toBe('')
  })
})

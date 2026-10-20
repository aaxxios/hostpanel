/**
 * Unit tests for NovaTestimonials: nothing without quotes, one card per quote read from the
 * storefront store with them.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { testimonialFixture } from '~/tests/helpers/storefrontFixtures'
import NovaTestimonials from '~/templates/nova/sections/NovaTestimonials.vue'

/** The mocked storefront endpoints, re-created for every test. */
const api = { fetchPartners: vi.fn(), fetchTestimonials: vi.fn(), fetchProjects: vi.fn(), fetchFaqs: vi.fn() }

vi.mock('~/composables/apis/useStorefrontApi', () => ({
  /**
   * Stands in for the storefront API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useStorefrontApi: () => api,
}))

/**
 * Mounts the section and lets the quotes load.
 *
 * @returns The wrapper.
 */
const mountSection = async () => {
  const wrapper = mountWithKit(NovaTestimonials)
  await flushPromises()
  return wrapper
}

describe('NovaTestimonials', () => {
  beforeEach(() => {
    api.fetchTestimonials.mockReset()
    api.fetchTestimonials.mockResolvedValue([
      testimonialFixture(1, { name: 'Ada', role: 'CTO', company: 'Example Co', quote: 'Fast.' }),
      testimonialFixture(2, { name: 'Bo', role: null, company: null, quote: 'Solid.' }),
    ])
  })

  it('renders no section while there are no quotes', async () => {
    api.fetchTestimonials.mockResolvedValue([])

    const wrapper = await mountSection()

    expect(wrapper.find('h2').exists()).toBe(false)
    expect(wrapper.find('li').exists()).toBe(false)
  })

  it('renders each quote with its attribution', async () => {
    const cards = (await mountSection()).findAll('li')

    expect(cards).toHaveLength(2)
    expect(cards[0]!.find('blockquote').text()).toBe('Fast.')
    expect(cards[0]!.text()).toContain('CTO, Example Co')
    expect(cards[1]!.text()).toContain('Bo')
    expect(cards[1]!.text()).not.toContain('CTO')
  })

  it('keeps cards at a normal column width, centred, instead of stretching them', async () => {
    const list = (await mountSection()).find('ul')

    expect(list.classes()).toContain('justify-center')
    expect(list.find('li').classes()).toContain('lg:w-[calc((100%-2.5rem)/3)]')
  })
})

/**
 * Unit tests for NovaFaq: one native disclosure per question read from the storefront store, and
 * nothing at all when there are no questions.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { faqFixture } from '~/tests/helpers/storefrontFixtures'
import NovaFaq from '~/templates/nova/sections/NovaFaq.vue'

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
 * Mounts the section and lets the questions load.
 *
 * @returns The wrapper.
 */
const mountSection = async () => {
  const wrapper = mountWithKit(NovaFaq)
  await flushPromises()
  return wrapper
}

describe('NovaFaq', () => {
  beforeEach(() => {
    api.fetchFaqs.mockReset()
    api.fetchFaqs.mockResolvedValue([faqFixture(1), faqFixture(2)])
  })

  it('renders a details element per question with its question and answer', async () => {
    const items = (await mountSection()).findAll('details')

    expect(items).toHaveLength(2)
    expect(items[0]!.find('h3').text()).toBe('Example question 1?')
    expect(items[0]!.text()).toContain('Example answer 1.')
  })

  it('gives every item a test id built from its id', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="nova-faq-2"]').exists()).toBe(true)
  })

  it('renders no section while there are no questions', async () => {
    api.fetchFaqs.mockResolvedValue([])

    const wrapper = await mountSection()

    expect(wrapper.find('details').exists()).toBe(false)
    expect(wrapper.find('h2').exists()).toBe(false)
  })
})

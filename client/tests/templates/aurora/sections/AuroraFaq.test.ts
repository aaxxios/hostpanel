/**
 * Unit tests for AuroraFaq: the questions come from the storefront store, every answer stays in
 * the page for crawlers, the first one is open, toggling follows the accordion, and the section
 * hides itself when there are no questions.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { faqFixture } from '~/tests/helpers/storefrontFixtures'
import AuroraFaq from '~/templates/aurora/sections/AuroraFaq.vue'
import { stubAuroraGlobals } from '../auroraGlobals'

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
  stubAuroraGlobals()
  const wrapper = mountWithKit(AuroraFaq)
  await flushPromises()
  return wrapper
}

describe('AuroraFaq', () => {
  beforeEach(() => {
    api.fetchFaqs.mockReset()
    api.fetchFaqs.mockResolvedValue([
      faqFixture(1, { question: 'How fast?', answer: 'Very fast.' }),
      faqFixture(2, { question: 'Is it safe?', answer: 'Yes, safe.' }),
    ])
  })

  it('renders every question and keeps every answer in the markup', async () => {
    const wrapper = await mountSection()

    expect(wrapper.text()).toContain('How fast?')
    expect(wrapper.text()).toContain('Is it safe?')
    expect(wrapper.html()).toContain('Very fast.')
    expect(wrapper.html()).toContain('Yes, safe.')
  })

  it('opens the first answer and hides the second', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="ui-accordion-toggle-0"]').attributes('aria-expanded')).toBe('true')
    expect(wrapper.find('[data-testid="ui-accordion-toggle-1"]').attributes('aria-expanded')).toBe('false')
  })

  it('opens another answer when its question is clicked', async () => {
    const wrapper = await mountSection()

    await wrapper.find('[data-testid="ui-accordion-toggle-1"]').trigger('click')

    expect(wrapper.find('[data-testid="ui-accordion-toggle-1"]').attributes('aria-expanded')).toBe('true')
    expect(wrapper.find('[data-testid="ui-accordion-toggle-0"]').attributes('aria-expanded')).toBe('false')
  })

  it('renders nothing when there are no questions', async () => {
    api.fetchFaqs.mockResolvedValue([])

    const wrapper = await mountSection()

    expect(wrapper.find('#faq').exists()).toBe(false)
    expect(wrapper.find('[data-testid="aurora-faq"]').exists()).toBe(false)
  })
})

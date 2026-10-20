/**
 * Unit tests for AuroraProjects: the projects come from the storefront store, show title, summary, link and image or a placeholder, and the section hides when there are none.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { projectFixture } from '~/tests/helpers/storefrontFixtures'
import AuroraProjects from '~/templates/aurora/sections/AuroraProjects.vue'
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
 * Mounts the section and lets the content load.
 *
 * @returns The wrapper.
 */
const mountSection = async () => {
  stubAuroraGlobals()
  const wrapper = mountWithKit(AuroraProjects)
  await flushPromises()
  return wrapper
}

describe('AuroraProjects', () => {
  beforeEach(() => {
    api.fetchProjects.mockReset()
    api.fetchProjects.mockResolvedValue([
      projectFixture(1, { title: 'Shop', summary: 'An online shop', imageUrls: ['https://cdn.example.test/s.png'], url: 'https://shop.example.test' }),
      projectFixture(2, { title: 'Blog', summary: null, imageUrls: [], url: null }),
    ])
  })

  it('renders a card per project with title, summary and link', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="project-1"]').text()).toContain('Shop')
    expect(wrapper.find('[data-testid="project-1"]').text()).toContain('An online shop')
    expect(wrapper.find('[data-testid="project-link-1"]').attributes('href')).toBe('https://shop.example.test')
    expect(wrapper.find('[data-testid="project-2"]').text()).toContain('Blog')
    expect(wrapper.find('[data-testid="project-link-2"]').exists()).toBe(false)
  })

  it('shows the first image, or a placeholder when the project has none', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="project-image-1"]').attributes('src')).toBe('https://cdn.example.test/s.png')
    expect(wrapper.find('[data-testid="project-placeholder-1"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="project-image-2"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="project-placeholder-2"]').text()).toBe('B')
  })

  it('renders nothing while there are no projects', async () => {
    api.fetchProjects.mockResolvedValue([])

    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="aurora-projects"]').exists()).toBe(false)
    expect(wrapper.find('h2').exists()).toBe(false)
  })

  it('gives every project card the shared resting depth', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="project-1"]').classes()).toContain('shadow-card')
  })
})

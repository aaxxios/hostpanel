/**
 * Unit tests for AuroraProjects: the projects come from the storefront store, show title, summary, link and image or a placeholder, and the section hides when there are none.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { projectFixture } from '~/tests/helpers/storefrontFixtures'
import AuroraProjects from '~/templates/aurora/sections/AuroraProjects.vue'
import { stubAuroraGlobals } from '../auroraGlobals'
import { balancedColumns } from '~/utils/balancedGrid'

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

  it('lays the cards out in the shared balanced grid, so no card is left alone on a row', async () => {
    api.fetchProjects.mockResolvedValue(Array.from({ length: 5 }, (_, index) => projectFixture(index + 1)))

    const wrapper = await mountSection()

    const grid = wrapper.find('[data-testid="aurora-projects-grid"]')
    expect(grid.exists()).toBe(true)
    expect(grid.attributes('data-columns')).toBe(String(balancedColumns(5)))
    expect(grid.classes()).toContain('ui-balanced-grid')
  })

  it('fills a project without an image with the patterned cover, initials and a site type icon', async () => {
    api.fetchProjects.mockResolvedValue([projectFixture(3, { title: 'Pine Shop', slug: 'pine-shop', imageUrls: [] })])

    const wrapper = await mountSection()

    const cover = wrapper.find('[data-testid="project-placeholder-3"]')
    expect(cover.find('[data-testid="ui-cover-fallback-initials"]').text()).toBe('PS')
    expect(cover.find('[data-testid="ui-cover-fallback-icon"]').attributes('data-site-type')).toBe('shop')
  })

  it('reads the shared storefront heading keys', async () => {
    const wrapper = await mountSection()

    expect(wrapper.text()).toContain('storefront.projects.title')
  })
})

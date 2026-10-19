/**
 * Unit tests for the trial page: it takes the product's colour, icon and page from its catalogue group,
 * reads its feature lines from the catalogue, and falls back to a neutral look for an unknown slug.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountPage, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import type { PublicPageSpies } from '~/tests/helpers/publicPage'
import TrialPage from '~/pages/trial/[product].vue'
import CatalogTrialHero from '~/components/catalog/TrialHero.vue'
import CatalogTrialDetails from '~/components/catalog/TrialDetails.vue'
import type { PortalProduct } from '~/types/portalproduct'
import type { PortalProductGroup } from '~/types/portalproductgroup'
import { familyColor } from '~/utils/catalogGroups'

/** The mocked catalogue endpoints, re-created for every test. */
const api = { fetchProducts: vi.fn(), fetchProductGroups: vi.fn() }

vi.mock('~/composables/apis/useCatalogApi', () => ({
  /**
   * Stands in for the catalogue API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useCatalogApi: () => api,
}))

vi.mock('~/composables/apis/useCurrencyApi', () => ({
  /**
   * Stands in for the currency API composable.
   *
   * @returns Only the endpoint the page's store reaches for.
   */
  useCurrencyApi: () => ({ loadBillingCurrencies: () => Promise.resolve([]) }),
}))

/** The product's group. */
const groups: PortalProductGroup[] = [{ id: 9, name: 'Gamma Tool', isActive: true, productCount: 1 }]

/** The product's catalogue entry. */
const catalogue: PortalProduct[] = [
  { id: 90, pid: 90, groupId: 9, name: 'Gamma Tool', description: 'Tasks<br />✔ Boards<br />✔ Reports' },
]

/** What the stood-in runtime recorded for the current test. */
let spies: PublicPageSpies

/**
 * Mounts the page for one product slug.
 *
 * @param product The `product` route parameter.
 * @returns The wrapper.
 */
const mountTrial = async (product = 'gamma-tool') => {
  spies = stubPublicPageRuntime({ params: { product } })
  const wrapper = await mountPage(TrialPage, { components: { CatalogTrialHero, CatalogTrialDetails } })
  await flushPromises()
  return wrapper
}

describe('trial page', () => {
  beforeEach(() => {
    api.fetchProducts.mockReset()
    api.fetchProducts.mockResolvedValue(catalogue)
    api.fetchProductGroups.mockReset()
    api.fetchProductGroups.mockResolvedValue(groups)
    window.localStorage.clear()
  })

  it("reads the products of the family groups", async () => {
    await mountTrial()

    expect(api.fetchProducts).toHaveBeenCalledWith(expect.objectContaining({ gids: '9' }))
  })

  it("paints the page in the product's colour and sends the trial to its page", async () => {
    const wrapper = await mountTrial()

    const accent = wrapper.find('[data-testid="ui-accent"]').element as HTMLElement
    expect(accent.style.getPropertyValue('--accent')).toBe(familyColor('gamma-tool'))
    expect(wrapper.find('[data-testid="trial-cta"]').attributes('href')).toBe('/products/gamma-tool')
  })

  it('lists the feature lines parsed from the catalogue description', async () => {
    const wrapper = await mountTrial()

    expect(wrapper.find('[data-testid="trial-features"]').text()).toContain('Boards')
    expect(wrapper.find('[data-testid="trial-features"]').text()).toContain('Reports')
  })

  it('shows the three trial perks', async () => {
    const wrapper = await mountTrial()

    expect(wrapper.findAll('[data-testid^="trial-perk-"]')).toHaveLength(3)
  })

  it('falls back to a neutral look and the products list for an unknown slug', async () => {
    api.fetchProductGroups.mockResolvedValue([])
    api.fetchProducts.mockResolvedValue([])
    const wrapper = await mountTrial('mystery')

    const accent = wrapper.find('[data-testid="ui-accent"]').element as HTMLElement
    expect(accent.style.getPropertyValue('--accent')).toBe('#6366f1')
    expect(wrapper.find('[data-testid="trial-learn-more"]').attributes('href')).toBe('/en/products')
    expect(wrapper.find('[data-testid="trial-features"]').exists()).toBe(false)
  })

  it('sets the head for the product', async () => {
    await mountTrial()

    expect(spies.useSeo).toHaveBeenCalledWith(expect.objectContaining({ path: '/trial/gamma-tool' }))
  })
})

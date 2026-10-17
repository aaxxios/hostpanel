/**
 * Unit tests for the hosting plan page: it finds the plan named by the address, prices it in the
 * payer's currency for the chosen cycle, adds it to the cart (a free plan at no charge) and answers
 * an unknown plan with a notice.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountPage, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import HostingPlanPage from '~/pages/hosting/[plan].vue'
import CatalogHostingPlanHero from '~/components/catalog/HostingPlanHero.vue'
import CatalogHostingPlanDetails from '~/components/catalog/HostingPlanDetails.vue'
import { useCartStore } from '~/stores/cart'
import type { BillingCurrency } from '~/types/currency'
import type { PortalProduct } from '~/types/portalproduct'

/** The mocked catalogue endpoints, re-created for every test. */
const api = { fetchProducts: vi.fn() }

/** The currency the stood-in currency endpoint answers with. */
const usd: BillingCurrency = { code: 'USD', numeric: '840', prefix: '$', suffix: '', decimals: 2, isBase: true }

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
  useCurrencyApi: () => ({ loadBillingCurrencies: () => Promise.resolve([usd]) }),
}))

/** The hosting plans the catalogue answers with. */
const plans: PortalProduct[] = [
  { id: 2, pid: 2, groupId: 1, name: 'Starter', description: 'Small sites<br />✔ 5 GB SSD<br />✔ Free SSL', pricing: { monthly: 5, annual: 50 } },
  { id: 3, pid: 3, groupId: 1, name: 'Free Trial', description: 'Try it', pricing: { monthly: null, annual: null } },
  { id: 4, pid: 4, groupId: 1, name: 'Custom', description: 'Ask us', pricing: { monthly: null, annual: null } },
]

/** The `useHead` stand-in, so a test reads the links the page publishes. */
const useHeadSpy = vi.fn()

/**
 * Mounts the page for one plan address.
 *
 * @param plan The `plan` route parameter.
 * @returns The wrapper.
 */
const mountPlan = async (plan = 'starter') => {
  stubPublicPageRuntime({ params: { plan } })
  vi.stubGlobal('useHead', useHeadSpy)
  vi.stubGlobal('useRuntimeConfig', () => ({ public: { baseUrl: 'https://panel.test' } }))
  const wrapper = await mountPage(HostingPlanPage, { components: { CatalogHostingPlanHero, CatalogHostingPlanDetails } })
  await flushPromises()
  return wrapper
}

describe('hosting plan page', () => {
  beforeEach(() => {
    api.fetchProducts.mockReset()
    api.fetchProducts.mockResolvedValue(plans)
    window.localStorage.clear()
  })

  it('builds the canonical link from the configured site URL', async () => {
    await mountPlan()

    const head = (useHeadSpy.mock.calls[0]?.[0] as () => { link: { rel: string, href: string }[] })()
    expect(head.link[0]).toEqual({ rel: 'canonical', href: 'https://panel.test/hosting/starter' })
  })

  it('finds the plan by its slug and shows its name, summary and monthly price', async () => {
    const wrapper = await mountPlan()

    expect(wrapper.find('[data-testid="hosting-plan-name"]').text()).toBe('Starter')
    expect(wrapper.find('[data-testid="hosting-plan-summary"]').text()).toBe('Small sites')
    expect(wrapper.find('[data-testid="hosting-plan-price"]').text()).toContain('5.00')
  })

  it('also finds the plan by its id', async () => {
    const wrapper = await mountPlan('2')

    expect(wrapper.find('[data-testid="hosting-plan-name"]').text()).toBe('Starter')
  })

  it('lists the included features and both billing cycles', async () => {
    const wrapper = await mountPlan()

    expect(wrapper.find('[data-testid="hosting-plan-features"]').text()).toContain('Free SSL')
    expect(wrapper.find('[data-testid="hosting-plan-cycle-monthly"]').text()).toContain('5.00')
    expect(wrapper.find('[data-testid="hosting-plan-cycle-annual"]').text()).toContain('50.00')
  })

  it('switches the headline price to the annual cycle', async () => {
    const wrapper = await mountPlan()

    await wrapper.find('[data-testid="hosting-plan-cycle-annual"]').trigger('click')

    expect(wrapper.find('[data-testid="hosting-plan-price"]').text()).toContain('50.00')
  })

  it('adds the plan to the cart in the selected cycle, and then shows it as in the cart', async () => {
    const wrapper = await mountPlan()
    await wrapper.find('[data-testid="hosting-plan-cycle-annual"]').trigger('click')

    await wrapper.find('[data-testid="hosting-plan-order"]').trigger('click')

    expect(useCartStore().items[0]).toMatchObject({ pid: 2, name: 'Starter', billingcycle: 'annual', amount: 50, currency: 'USD' })
    expect(wrapper.find('[data-testid="hosting-plan-order"]').text()).toContain('hosting.inCart')
  })

  it('adds a plan whose name declares it free at no charge', async () => {
    const wrapper = await mountPlan('free-trial')

    await wrapper.find('[data-testid="hosting-plan-order"]').trigger('click')

    expect(useCartStore().items[0]).toMatchObject({ pid: 3, billingcycle: 'free', amount: 0, cycleLabel: 'hosting.cycles.free' })
  })

  it('shows "custom" and adds nothing for a plan with no price that is not free', async () => {
    const wrapper = await mountPlan('custom')
    expect(wrapper.find('[data-testid="hosting-plan-price"]').text()).toBe('hosting.custom')

    await wrapper.find('[data-testid="hosting-plan-order"]').trigger('click')

    expect(useCartStore().items).toEqual([])
  })

  it('answers an unknown plan with a notice and no details', async () => {
    const wrapper = await mountPlan('no-such-plan')

    expect(wrapper.find('[data-testid="hosting-plan-missing"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="hosting-plan-details"]').exists()).toBe(false)
  })
})

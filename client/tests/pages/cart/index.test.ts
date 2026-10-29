/**
 * Unit tests for the cart page: it shows the visitor's lines in the currency each was added in,
 * totals those in the payer's currency, flags the others, and removes or clears lines.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { nextTick } from 'vue'
import { mountPage, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import CartPage from '~/pages/cart/index.vue'
import CartHero from '~/components/cart/CartHero.vue'
import CartContent from '~/components/cart/CartContent.vue'
import { useCartStore } from '~/stores/cart'
import type { BillingCurrency } from '~/types/currency'
import { PricingCycle } from '~/types/pricingcycle'

/** The currencies the stood-in currency endpoint answers with. */
const currencies: BillingCurrency[] = [
  { code: 'USD', numeric: '840', prefix: '$', suffix: '', decimals: 2, isBase: true },
  { code: 'AMD', numeric: '051', prefix: '', suffix: ' AMD', decimals: 0, isBase: false },
]

vi.mock('~/composables/apis/useCurrencyApi', () => ({
  /**
   * Stands in for the currency API composable.
   *
   * @returns Only the endpoint the page's store reaches for.
   */
  useCurrencyApi: () => ({ loadBillingCurrencies: () => Promise.resolve(currencies) }),
}))

/**
 * Mounts the page and lets the currencies load.
 *
 * @returns The wrapper.
 */
const mountCart = async () => {
  stubPublicPageRuntime()
  const wrapper = await mountPage(CartPage, { components: { CartHero, CartContent } })
  await flushPromises()
  return wrapper
}

/**
 * Puts a line in the cart and waits for the page to render it.
 *
 * @param pid The product id.
 * @param name The line name.
 * @param amount The price.
 * @param currency The currency the price is in.
 */
const addLine = async (pid: number, name: string, amount: number, currency: string): Promise<void> => {
  useCartStore().addItem({ pid, name, billingcycle: PricingCycle.Monthly, cycleLabel: 'Monthly', amount, currency })
  await nextTick()
}

describe('cart page', () => {
  beforeEach(() => {
    window.localStorage.clear()
  })

  it('shows the empty notice before anything is added', async () => {
    const wrapper = await mountCart()

    expect(wrapper.find('[data-testid="cart-empty"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="cart-item"]').exists()).toBe(false)
  })

  it('lists a line and totals it in the payer currency', async () => {
    const wrapper = await mountCart()

    await addLine(1, 'Starter', 5, 'USD')

    expect(wrapper.findAll('[data-testid="cart-item"]')).toHaveLength(1)
    expect(wrapper.find('[data-testid="cart-item-name"]').text()).toBe('Starter')
    expect(wrapper.find('[data-testid="cart-total"]').text()).toContain('5.00')
    expect(wrapper.find('[data-testid="cart-empty"]').exists()).toBe(false)
  })

  it('formats a line in the currency it was added in and warns when it is not the payer currency', async () => {
    const wrapper = await mountCart()

    await addLine(2, 'Business', 2000, 'AMD')

    expect(wrapper.find('[data-testid="cart-item"]').text()).toContain('AMD')
    expect(wrapper.find('[data-testid="cart-stale"]').exists()).toBe(true)
  })

  it('removes the line the visitor removes', async () => {
    const wrapper = await mountCart()
    await addLine(1, 'Starter', 5, 'USD')
    await addLine(3, 'Pro', 9, 'USD')

    await wrapper.findAll('[data-testid="cart-item-remove"]')[0]?.trigger('click')

    expect(useCartStore().items.map((item) => item.pid)).toEqual([3])
  })

  it('empties the cart from Clear cart', async () => {
    const wrapper = await mountCart()
    await addLine(1, 'Starter', 5, 'USD')

    await wrapper.find('[data-testid="cart-clear"]').trigger('click')

    expect(useCartStore().items).toEqual([])
    expect(wrapper.find('[data-testid="cart-empty"]').exists()).toBe(true)
  })

  it('leads to checkout', async () => {
    const wrapper = await mountCart()
    await addLine(1, 'Starter', 5, 'USD')

    expect(wrapper.find('[data-testid="cart-checkout"]').attributes('href')).toBe('/en/checkout')
  })
})

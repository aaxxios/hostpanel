/**
 * Tests for `composables/useCheckoutPage.ts` with the cart, currency, checkout and auth stores
 * replaced by minimal fakes: the guest form, the signed-in line, the currency options and the
 * place-order flow that both checkout templates share.
 *
 * @module tests/composables/useCheckoutPage.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { Suspense, defineComponent, h, reactive, ref } from 'vue'
import { flushPromises, mount } from '@vue/test-utils'
import { CheckoutDestinationKind } from '~/types/checkoutdestinationkind'
import { useCheckoutPage } from '~/composables/useCheckoutPage'

/** The fake stores and their spies, shared with the module mocks below. */
const fakes = vi.hoisted(() => ({
  cart: null as null | Record<string, unknown>,
  currency: null as null | Record<string, unknown>,
  checkout: null as null | Record<string, unknown>,
  auth: null as null | Record<string, unknown>,
}))

vi.mock('~/stores/cart', () => ({ useCartStore: () => fakes.cart }))
vi.mock('~/stores/currency', () => ({ useCurrencyStore: () => fakes.currency }))
vi.mock('~/stores/checkout', () => ({ useCheckoutStore: () => fakes.checkout }))
vi.mock('~/stores/auth', () => ({ useAuthStore: () => fakes.auth }))

/**
 * Builds the four fake stores.
 *
 * @param destination What the checkout store's order action answers with.
 * @returns The spies a test asserts on.
 */
const buildStores = (destination: { kind: CheckoutDestinationKind, target: string } | null) => {
  const placeOrder = vi.fn(async () => destination)
  const changeGuestCurrency = vi.fn()
  fakes.cart = reactive({ init: vi.fn() })
  fakes.currency = reactive({ currencies: [{ code: 'USD' }, { code: 'EUR' }], init: vi.fn(), load: vi.fn() })
  fakes.checkout = {
    staleItems: ref([]),
    totalLabel: ref(''),
    submittingLabel: ref(''),
    paymentMethods: ref([]),
    selectedMethod: ref(''),
    submitting: ref(false),
    error: ref(''),
    itemPrice: () => '',
    changeGuestCurrency,
    loadPaymentMethods: vi.fn(async () => undefined),
    placeOrder,
  }
  fakes.auth = { user: ref({ email: 'ann@acme.test' }), isLoggedIn: ref(true), fetchUser: vi.fn(), logout: vi.fn() }
  return { placeOrder, changeGuestCurrency }
}

/**
 * Runs the composable the way a page does: inside a component's setup, under `Suspense`, so the
 * lifecycle hook it registers has an instance to belong to and fires on mount.
 *
 * @returns What the composable returned.
 */
const runInPage = async () => {
  /** The composable's result, captured from inside the host's setup. */
  let page: Awaited<ReturnType<typeof useCheckoutPage>> | undefined
  const Host = defineComponent({
    async setup() {
      page = await useCheckoutPage()
      return () => h('div')
    },
  })
  mount(defineComponent({ render: () => h(Suspense, null, { default: () => h(Host) }) }))
  await flushPromises()
  if (!page) {
    throw new Error('the composable did not settle')
  }
  return page
}

describe('useCheckoutPage', () => {
  beforeEach(() => {
    vi.stubGlobal('useHead', vi.fn())
    vi.stubGlobal('useLocalePath', () => (path: string) => path)
  })

  it('offers the currencies as select options and the signed-in e-mail', async () => {
    buildStores(null)
    const page = await runInPage()

    expect(page.currencyOptions.value).toEqual([{ value: 'USD', label: 'USD' }, { value: 'EUR', label: 'EUR' }])
    expect(page.userEmail.value).toBe('ann@acme.test')
    expect(fakes.cart?.init).toHaveBeenCalled()
    expect(fakes.currency?.load).toHaveBeenCalled()
  })

  it('asks the visitor before the store changes the currency', async () => {
    const { changeGuestCurrency } = buildStores(null)
    const page = await runInPage()

    page.changeGuestCurrency('EUR')

    expect(changeGuestCurrency).toHaveBeenCalledWith('EUR', expect.any(Function))
  })

  it('sends the guest form with the order and goes to the route the store names', async () => {
    const { placeOrder } = buildStores({ kind: CheckoutDestinationKind.Route, target: '/client/order-success' })
    const page = await runInPage()
    page.form.email = 'guest@acme.test'

    await page.submitOrder()

    expect(placeOrder).toHaveBeenCalledWith(expect.objectContaining({ guest: expect.objectContaining({ email: 'guest@acme.test' }) }))
    expect(navigateTo).toHaveBeenCalledWith('/client/order-success')
  })

  it('stays put when the store refuses the order', async () => {
    buildStores(null)
    vi.mocked(navigateTo).mockClear()
    const page = await runInPage()

    await page.submitOrder()

    expect(navigateTo).not.toHaveBeenCalled()
  })
})

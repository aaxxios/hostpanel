/**
 * Unit tests for ClassicCheckout: the empty cart, the order summary, the account step in both
 * states, the payment-method choice and the card form, placing an order and a refused order.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { defineComponent, h } from 'vue'
import { flushPromises } from '@vue/test-utils'
import { mountPage, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import ClassicCheckout from '~/templates/classic/pages/ClassicCheckout.vue'
import { useAuthStore } from '~/stores/auth'
import { useCartStore } from '~/stores/cart'
import type { BillingCurrency } from '~/types/currency'
import type { CartItem } from '~/types/cartitem'
import type { ClientUser } from '~/types/clientuser'
import { PAYMENT_MODULE_BANK_TRANSFER, PAYMENT_MODULE_STRIPE } from '~/utils/paymentModules'

/** The mocked billing endpoints, re-created for every test. */
const billing = { fetchGatewayMethods: vi.fn(), createOrder: vi.fn() }

/** The mocked sign-in call, used for the guest's automatic sign-in after an order. */
const loginMock = vi.fn()

/** The currency the stood-in currency endpoint answers with. */
const usd: BillingCurrency = { code: 'USD', numeric: '840', prefix: '$', suffix: '', decimals: 2, isBase: true }

vi.mock('~/composables/apis/useBillingApi', () => ({
  /**
   * Stands in for the billing API composable so the stores run without transport.
   *
   * @returns The mocked endpoints.
   */
  useBillingApi: () => billing
}))

vi.mock('~/composables/apis/useAuthApi', () => ({
  /**
   * Stands in for the auth API composable.
   *
   * @returns Only the endpoint the checkout reaches for.
   */
  useAuthApi: () => ({ login: loginMock, logout: vi.fn() })
}))

vi.mock('~/composables/apis/useCurrencyApi', () => ({
  /**
   * Stands in for the currency API composable.
   *
   * @returns Only the endpoint the currency store reaches for.
   */
  useCurrencyApi: () => ({ loadBillingCurrencies: () => Promise.resolve([usd]) })
}))

vi.mock('~/composables/apis/useClientApi', () => ({
  /**
   * Stands in for the client API composable; the checkout calls none of its endpoints.
   *
   * @returns An empty set of endpoints.
   */
  useClientApi: () => ({})
}))

vi.mock('~/composables/useOrderPaymentToken', () => ({
  /**
   * Stands in for the payment-token memory, which needs the cookie plugin.
   *
   * @returns A remember function that does nothing.
   */
  useOrderPaymentToken: () => ({ remember: vi.fn(), recall: vi.fn(), forget: vi.fn() })
}))

/** Stand-in for the card form, which mounts the payment provider's element. */
const StripeCardFormStub = defineComponent({
  name: 'CheckoutStripeCardForm',
  setup: (_props, { expose }) => {
    expose({ confirmPayment: () => Promise.resolve({ id: 'pi_1' }) })
    return () => h('div', { 'data-testid': 'card-form-stub' })
  },
})

/** Stand-in for the phone field, which wraps a third-party widget. */
const PhoneInputStub = defineComponent({
  name: 'UiPhoneInput',
  props: { modelValue: { type: String, default: '' } },
  emits: ['update:modelValue'],
  setup: (props, { emit }) => () => h('input', {
    'data-testid': 'checkout-phone',
    value: props.modelValue,
    onInput: (event: Event) => emit('update:modelValue', (event.target as HTMLInputElement).value),
  }),
})

/** A signed-in client. */
const client = { id: 1, firstname: 'Ada', lastname: 'Lovelace', email: 'ada@example.test' } as ClientUser

/**
 * Builds a hosting cart line.
 *
 * @param pid Product id.
 * @param amount Price of the line.
 * @param currency Currency the line is priced in.
 * @returns The line.
 */
const line = (pid: number, amount: number, currency = 'USD'): CartItem => ({
  pid,
  name: `Plan ${pid}`,
  billingcycle: 'monthly',
  cycleLabel: 'Monthly',
  amount,
  currency,
})

/**
 * Mounts the checkout, fills the cart and lets the currencies and gateways load.
 *
 * @param items Lines to put in the cart.
 * @returns The wrapper.
 */
const mountCheckout = async (items: CartItem[] = [line(1, 5), line(2, 7)]) => {
  const wrapper = await mountPage(ClassicCheckout, {
    components: { CheckoutStripeCardForm: StripeCardFormStub, UiPhoneInput: PhoneInputStub },
  })
  useCartStore().items = items
  await flushPromises()
  return wrapper
}

/**
 * Fills the guest form.
 *
 * @param wrapper The mounted page.
 */
const fillGuestForm = async (wrapper: Awaited<ReturnType<typeof mountCheckout>>) => {
  await wrapper.find('[data-testid="checkout-firstname"]').setValue('Ada')
  await wrapper.find('[data-testid="checkout-lastname"]').setValue('Lovelace')
  await wrapper.find('[data-testid="checkout-email"]').setValue('ada@example.test')
  await wrapper.find('[data-testid="checkout-password"]').setValue('s3cret-pass')
  await wrapper.find('[data-testid="checkout-phone"]').setValue('+37400000000')
}

describe('ClassicCheckout', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
    billing.fetchGatewayMethods.mockReset()
    billing.fetchGatewayMethods.mockResolvedValue([
      { module: PAYMENT_MODULE_BANK_TRANSFER, displayName: 'Bank transfer' },
      { module: PAYMENT_MODULE_STRIPE, displayName: 'Card' },
    ])
    billing.createOrder.mockReset()
    billing.createOrder.mockResolvedValue({ orderId: 7, invoiceId: 12, paymentToken: 'token' })
    loginMock.mockReset()
    loginMock.mockResolvedValue(client)
    vi.mocked(navigateTo).mockClear()
    window.localStorage.clear()
  })

  it('shows the empty state with a way to the hosting plans when the cart is empty', async () => {
    const wrapper = await mountCheckout([])

    expect(wrapper.find('[data-testid="checkout-empty"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="checkout-browse"]').attributes('href')).toBe('/en/hosting')
    expect(wrapper.find('[data-testid="checkout-grid"]').exists()).toBe(false)
  })

  it('lists the cart lines and the total in the order summary', async () => {
    const wrapper = await mountCheckout()

    const summary = wrapper.find('[data-testid="checkout-summary"]')
    expect(summary.findAll('[data-testid="checkout-item"]')).toHaveLength(2)
    expect(summary.text()).toContain('$5.00')
    expect(wrapper.find('[data-testid="checkout-total"]').text()).toBe('$12.00')
  })

  it('warns about lines left in another currency and leaves them out', async () => {
    const wrapper = await mountCheckout([line(1, 5), line(2, 2000, 'AMD')])

    expect(wrapper.find('[data-testid="checkout-stale"]').exists()).toBe(true)
    expect(wrapper.findAll('[data-testid="checkout-item"]')).toHaveLength(1)
  })

  it('shows the guest form to a visitor, every text field with its placeholder', async () => {
    const wrapper = await mountCheckout()

    expect(wrapper.find('[data-testid="checkout-guest-form"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="checkout-firstname"]').attributes('placeholder')).toBe('checkout.placeholders.firstname')
    expect(wrapper.find('[data-testid="checkout-lastname"]').attributes('placeholder')).toBe('checkout.placeholders.lastname')
    expect(wrapper.find('[data-testid="checkout-email"]').attributes('placeholder')).toBe('checkout.placeholders.email')
    expect(wrapper.find('[data-testid="checkout-password"]').attributes('placeholder')).toBe('checkout.placeholders.password')
    expect(wrapper.find('[data-testid="checkout-sign-in"]').attributes('href')).toBe('/en/client/login')
  })

  it('shows the signed-in client instead of the guest form', async () => {
    const wrapper = await mountCheckout()

    useAuthStore().user = client
    await flushPromises()

    expect(wrapper.find('[data-testid="checkout-guest-form"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="checkout-user-email"]').text()).toBe('ada@example.test')
  })

  it('lists the gateways with the first preselected', async () => {
    const wrapper = await mountCheckout()

    const bank = wrapper.find(`[data-testid="checkout-method-${PAYMENT_MODULE_BANK_TRANSFER}"]`)
    const card = wrapper.find(`[data-testid="checkout-method-${PAYMENT_MODULE_STRIPE}"]`)
    expect(bank.text()).toContain('Bank transfer')
    expect(card.text()).toContain('Card')
    expect(bank.classes()).toContain('bg-classic-brand-soft')
    expect(card.classes()).not.toContain('bg-classic-brand-soft')
  })

  it('draws with the classic tokens only, no gradients, glows or cyan', async () => {
    const wrapper = await mountCheckout()

    const html = wrapper.html()
    expect(html).not.toContain('bg-gradient')
    expect(html).not.toContain('cyan-')
    expect(html).not.toContain('blur-')
    expect(wrapper.find('[data-testid="checkout-summary"]').classes()).toContain('border-t-classic-brand')
  })

  it('says so when no gateway is on, and cannot be submitted', async () => {
    billing.fetchGatewayMethods.mockResolvedValue([])
    const wrapper = await mountCheckout()

    expect(wrapper.find('[data-testid="checkout-no-methods"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="checkout-submit"]').attributes('disabled')).toBeDefined()
  })

  it('opens the card form only for the card gateway', async () => {
    const wrapper = await mountCheckout()
    expect(wrapper.find('[data-testid="checkout-card-form"]').exists()).toBe(false)

    await wrapper.find(`[data-testid="checkout-method-${PAYMENT_MODULE_STRIPE}"]`).trigger('click')

    expect(wrapper.find('[data-testid="checkout-card-form"]').exists()).toBe(true)
  })

  it('places a bank-transfer order with the guest details and goes to the invoice', async () => {
    const wrapper = await mountCheckout()
    await fillGuestForm(wrapper)

    await wrapper.find('[data-testid="checkout-submit"]').trigger('click')
    await flushPromises()

    expect(billing.createOrder).toHaveBeenCalledWith(expect.objectContaining({
      paymentmethod: PAYMENT_MODULE_BANK_TRANSFER,
      currency: 'USD',
      firstname: 'Ada',
      lastname: 'Lovelace',
      email: 'ada@example.test',
      password: 's3cret-pass',
      phonenumber: '+37400000000',
      items: [
        expect.objectContaining({ productId: 1, billingcycle: 'monthly' }),
        expect.objectContaining({ productId: 2, billingcycle: 'monthly' }),
      ],
    }))
    expect(navigateTo).toHaveBeenCalledWith('/en/client/invoices/12/pay')
    expect(useCartStore().items).toEqual([])
  })

  it('shows the refusal sentence and stays on the page when the order is refused', async () => {
    billing.createOrder.mockRejectedValue({ data: { detail: 'Email already registered.' } })
    const wrapper = await mountCheckout()
    await fillGuestForm(wrapper)

    await wrapper.find('[data-testid="checkout-submit"]').trigger('click')
    await flushPromises()

    expect(wrapper.find('[data-testid="checkout-error"]').text()).toBe('Email already registered.')
    expect(navigateTo).not.toHaveBeenCalled()
    expect(useCartStore().items).toHaveLength(2)
  })

  it('offers the contact link in the help card', async () => {
    const wrapper = await mountCheckout()

    expect(wrapper.find('[data-testid="checkout-contact"]').attributes('href')).toBe('/en/contact')
  })
})

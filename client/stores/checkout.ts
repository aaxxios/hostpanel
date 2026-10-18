/**
 * Pinia store for the one order flow both checkout templates share.
 *
 * It owns the gateway list, the chosen method, the in-flight flag and the failure sentence, and
 * walks the order through its steps: place the order, sign a guest in, then settle it by card
 * (the page supplies the card element through {@link PlaceOrderInput.confirmCard}), by hosted
 * gateway or by bank transfer. It reports where the payer goes next as a
 * {@link CheckoutDestination} instead of navigating, so the page keeps the router and a test
 * needs none. Templates differ only in markup; the flow is here once.
 *
 * Required guest fields are refused by the API (field errors land in {@link fieldErrors}); the
 * store does not author a sentence of its own for them.
 *
 * URLs and transport belong to {@link useBillingApi}; this store owns only the state.
 *
 * @module stores/checkout
 */

import { defineStore } from 'pinia'
import { useBillingApi } from '~/composables/apis/useBillingApi'
import { useOrderPaymentToken } from '~/composables/useOrderPaymentToken'
import { apiErrorMessage, apiFieldErrors } from '~/lib/errorHandler'
import { useAuthStore } from '~/stores/auth'
import { useCartStore } from '~/stores/cart'
import { useCurrencyStore } from '~/stores/currency'
import type { CartItem } from '~/types/cartitem'
import type { CheckoutDestination } from '~/types/checkoutdestination'
import { CheckoutDestinationKind } from '~/types/checkoutdestinationkind'
import type { GatewayMethod } from '~/types/gatewaymethod'
import type { PlaceOrderInput } from '~/types/placeorderinput'
import { formatMoney } from '~/utils/formatMoney'
import { PAYMENT_MODULE_BANK_TRANSFER, PAYMENT_MODULE_STRIPE } from '~/utils/paymentModules'
import { translate } from '~/utils/translate'
import { CartItemType } from '~/types/cartitemtype'

/**
 * Central store for placing an order.
 *
 * @returns The checkout state and the actions that change it.
 */
export const useCheckoutStore = defineStore('checkout', () => {
  const billing = useBillingApi()
  const cart = useCartStore()
  const currencyStore = useCurrencyStore()
  const authStore = useAuthStore()
  const { remember: rememberPaymentToken } = useOrderPaymentToken()

  /** The gateways the order can be paid through; empty until {@link loadPaymentMethods}. */
  const paymentMethods = ref<GatewayMethod[]>([])

  /** Whether the gateway list is being read. */
  const methodsLoading = ref(false)

  /** The gateway module the payer picked; defaults to the first one offered. */
  const selectedMethod = ref('')

  /** Whether an order is being placed and settled. */
  const submitting = ref(false)

  /** The sentence for the last refused order; empty when there is none to show. */
  const error = ref('')

  /** Field-level messages of the last refused order, keyed by camelCase field name. */
  const fieldErrors = ref<Record<string, string>>({})

  /** Items priced in a currency other than the payer's current one -- excluded from the order. */
  const staleItems = computed(() => cart.staleItems(currencyStore.payerCode))

  /** The order total in the payer's currency, formatted. */
  const totalLabel = computed(() => {
    const currency = currencyStore.moneyCurrencyFor(currencyStore.payerCode)
    return formatMoney(cart.items.length ? cart.total(currencyStore.payerCode) : 0, currency)
  })

  /** Submit-button label while the order is being placed: a hosted gateway hands off to the bank. */
  const submittingLabel = computed(() =>
    selectedMethod.value !== PAYMENT_MODULE_STRIPE
      && selectedMethod.value !== PAYMENT_MODULE_BANK_TRANSFER
      && selectedMethod.value
      ? translate('checkout.redirectingToBank')
      : translate('checkout.processing'))

  /**
   * Formats one cart item's price, in the currency it was actually added in.
   *
   * @param item - The cart item.
   * @returns The formatted price.
   */
  const itemPrice = (item: CartItem): string =>
    formatMoney(item.amount, currencyStore.moneyCurrencyFor(item.currency))

  /**
   * Changes the guest's chosen checkout currency.
   *
   * Prices already in the cart were fetched in the previous currency and cannot be converted --
   * nothing on the frontend converts a stored price -- so the cart is cleared once the visitor
   * confirms, per `docs/superpowers/specs/2026-09-13-multi-currency-pricing-design.md` §5.
   *
   * @param code - The newly chosen currency code.
   * @param confirmClear - Asks the visitor to accept an emptied cart; only called when the cart has items.
   * @returns True when the currency changed.
   */
  const changeGuestCurrency = (code: string, confirmClear: () => boolean): boolean => {
    if (code === currencyStore.payerCode) {
      return false
    }
    if (cart.items.length && !confirmClear()) {
      return false
    }
    currencyStore.select(code)
    cart.clear()
    return true
  }

  /**
   * Reads the gateways an order may be paid through and preselects the first one.
   *
   * @returns Nothing; a failure leaves the list empty and {@link error} holding the sentence.
   */
  const loadPaymentMethods = async (): Promise<void> => {
    methodsLoading.value = true
    try {
      paymentMethods.value = await billing.fetchGatewayMethods()
      const first = paymentMethods.value[0]
      if (first && !selectedMethod.value) {
        selectedMethod.value = first.module
      }
    } catch (e: unknown) {
      paymentMethods.value = []
      error.value = apiErrorMessage(e)
    } finally {
      methodsLoading.value = false
    }
  }

  /**
   * Chooses the sentence for a failed step: the API's own, else the payment provider's, else a generic one.
   *
   * @param e - Whatever the step threw.
   * @returns The sentence to show.
   */
  const sentenceFor = (e: unknown): string => {
    const isApiRefusal = typeof e === 'object' && e !== null && 'data' in e
    if (isApiRefusal) {
      return apiErrorMessage(e)
    }
    // The card element throws `Error` carrying the payment provider's own sentence.
    return e instanceof Error && e.message ? e.message : translate('checkout.errors.generic')
  }

  /**
   * Places the order and settles it by the chosen method.
   *
   * @param input - The guest's details, the card confirmation and the path localiser.
   * @returns Where the payer goes next, or null when nothing was placed or the order was refused
   * ({@link error} and {@link fieldErrors} then hold the reason).
   */
  const placeOrder = async (input: PlaceOrderInput): Promise<CheckoutDestination | null> => {
    if (!selectedMethod.value || submitting.value) {
      return null
    }
    error.value = ''
    fieldErrors.value = {}

    const method = selectedMethod.value
    const isGuest = !authStore.isLoggedIn
    const body: Record<string, unknown> = {
      // Stale-currency items are never sent -- they are excluded from the total and flagged in
      // the summary for removal, and must not silently ride along into the order.
      items: cart.itemsInCurrency(currencyStore.payerCode).map(i => ({
        productId: i.pid,
        billingcycle: i.billingcycle,
        domain: i.domain || undefined,
        hostname: i.hostname || undefined,
        domainAction: i.domainAction || undefined,
        eppCode: i.eppCode || undefined,
        years: i.years || undefined,
      })),
      paymentmethod: method,
      // The guest's chosen currency; ignored by the backend for an existing signed-in client,
      // whose currency is already fixed on their client record.
      currency: currencyStore.payerCode,
    }
    if (isGuest) {
      Object.assign(body, { ...input.guest })
    }

    submitting.value = true
    try {
      // Step 1: place the order (order and invoice are created on the backend).
      const result = await billing.createOrder(body)

      // Auto-sign-in the guest after the account was created. A refusal here must not fail an
      // order that already exists; the client-area middleware asks them to sign in later.
      if (isGuest && input.guest.email && input.guest.password) {
        try {
          await authStore.login(input.guest.email, input.guest.password)
        } catch {
          // The order is placed either way.
        }
      }

      if (method === PAYMENT_MODULE_STRIPE) {
        // Step 2: open the payment intent; step 3: confirm the card with the provider's element.
        const { clientSecret } = await billing.createOrderPaymentIntent(result.orderId, result.paymentToken)
        const paymentIntent = await input.confirmCard(clientSecret)

        // Step 4: tell the backend the payment succeeded -- it accepts the order and creates services.
        await billing.confirmOrderPayment(
          result.orderId,
          { paymentIntentId: paymentIntent.id },
          result.paymentToken
        )

        const domainItem = cart.items.find(i => i.itemType === CartItemType.Domain)
        const domainQuery = domainItem
          ? `&domain=${domainItem.domain}&action=${domainItem.domainAction}`
          : ''
        const finalAmount = totalLabel.value
        cart.clear()
        return {
          kind: CheckoutDestinationKind.Route,
          target: input.localizePath(
            `/client/order-success?order=ORD-${String(result.orderId).padStart(4, '0')}&amount=${finalAmount}${domainQuery}`
          )
        }
      }

      if (method !== PAYMENT_MODULE_BANK_TRANSFER) {
        // Any plugin-backed payment method is a hosted gateway: register the payment and hand
        // the browser to the bank. Branching on "not card, not bank transfer" rather than a
        // specific plugin id keeps a second gateway plugin from falling through to the
        // bank-transfer branch below. The order and invoice already exist, and the payer
        // returns via /payment/result, which verifies with the bank.
        const returnUrl = new URL(
          input.localizePath(`/payment/result?order=${result.orderId}`),
          window.location.origin,
        ).toString()
        // Stored before we leave for the bank, not after: the payer returns as a fresh
        // navigation that knows only the order id, and that page has to prove the order is
        // theirs before the backend will say whether the payment went through.
        rememberPaymentToken(result.orderId, result.paymentToken)
        const { redirectUrl } = await billing.startOrderGatewayPayment(
          result.orderId,
          method,
          returnUrl,
          result.paymentToken
        )
        cart.clear()
        return { kind: CheckoutDestinationKind.External, target: redirectUrl }
      }

      // Bank transfer: the order stays pending and the payer is sent to the invoice.
      cart.clear()
      return {
        kind: CheckoutDestinationKind.Route,
        target: input.localizePath(
          result.invoiceId ? `/client/invoices/${result.invoiceId}/pay` : '/client/invoices'
        )
      }
    } catch (e: unknown) {
      error.value = sentenceFor(e)
      fieldErrors.value = apiFieldErrors(e)
      return null
    } finally {
      submitting.value = false
    }
  }

  return {
    paymentMethods,
    methodsLoading,
    selectedMethod,
    submitting,
    error,
    fieldErrors,
    staleItems,
    totalLabel,
    submittingLabel,
    itemPrice,
    changeGuestCurrency,
    loadPaymentMethods,
    placeOrder
  }
})

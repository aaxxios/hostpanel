/**
 * Pinia store for one invoice: reading it, paying it from `pages/client/invoices/[id]/pay.vue`,
 * and verifying a hosted-gateway payment on `pages/payment/result.vue`.
 *
 * Owns the loaded invoice, the saved and hosted payment methods, the in-flight flag, the success
 * flag and the refusal sentence, so the pages render them and never wrap an API call in their
 * own try/catch. URLs and transport belong to {@link useBillingApi}.
 *
 * @module stores/invoicepayment
 */

import { defineStore } from 'pinia'
import { useBillingApi } from '~/composables/apis/useBillingApi'
import { ActionOutcome } from '~/types/actionoutcome'
import { apiErrorMessage } from '~/lib/errorHandler'
import type { ClientInvoice } from '~/types/clientinvoice'
import type { GatewayMethod } from '~/types/gatewaymethod'
import { GatewayPaymentState } from '~/types/gatewaypaymentstate'
import type { PaymentMethod } from '~/types/payment'
import { PAYMENT_MODULE_BANK_TRANSFER, PAYMENT_MODULE_STRIPE } from '~/utils/paymentModules'
import type { PaymentTarget } from '~/types/paymenttarget'

/** How the backend's settled-payment words map onto the page's states. */
const STATE_BY_WIRE: Record<'paid' | 'pending' | 'declined', GatewayPaymentState> = {
  paid: GatewayPaymentState.Paid,
  pending: GatewayPaymentState.Pending,
  declined: GatewayPaymentState.Declined
}

/**
 * Payment state for the invoice payment page.
 *
 * @returns The payment state and the actions that change it.
 */
export const useInvoicePaymentStore = defineStore('invoicePayment', () => {
  const billing = useBillingApi()

  /** The invoice on screen; null until {@link loadInvoice} or after a failed read. */
  const invoice = ref<ClientInvoice | null>(null)

  /** Whether the invoice is being read. */
  const invoiceLoading = ref(false)

  /** The sentence for the last failed invoice read; empty when there is none to show. */
  const invoiceError = ref('')

  /** The client's saved cards and bank accounts; empty until {@link loadPaymentOptions}. */
  const savedMethods = ref<PaymentMethod[]>([])

  /** The gateways an invoice can be paid through; empty until {@link loadPaymentOptions}. */
  const gatewayMethods = ref<GatewayMethod[]>([])

  /**
   * The first plugin-backed gateway (not a fixed module id, so a second gateway plugin is not
   * silently ignored), or null when the card and bank-transfer built-ins are all there is.
   */
  const hostedGatewayModule = computed<string | null>(() =>
    gatewayMethods.value
      .find(m => m.module !== PAYMENT_MODULE_STRIPE && m.module !== PAYMENT_MODULE_BANK_TRANSFER)?.module ?? null)

  /**
   * Reads one invoice with its line items.
   *
   * @param id - Invoice id.
   * @returns The invoice; null when it could not be read ({@link invoiceError} holds the sentence).
   */
  const loadInvoice = async (id: string): Promise<ClientInvoice | null> => {
    invoiceLoading.value = true
    invoiceError.value = ''
    invoice.value = null
    try {
      invoice.value = await billing.fetchInvoice(id)
    } catch (e: unknown) {
      invoiceError.value = apiErrorMessage(e)
    } finally {
      invoiceLoading.value = false
    }
    return invoice.value
  }

  /**
   * Reads what the payer can pay with: the saved methods and the available gateways. Either list
   * that cannot be read stays empty, which the page already renders as "nothing selectable".
   *
   * @returns Nothing.
   */
  const loadPaymentOptions = async (): Promise<void> => {
    const [saved, gateways] = await Promise.all([
      billing.fetchPaymentMethods().catch((): PaymentMethod[] => []),
      billing.fetchGatewayMethods().catch((): GatewayMethod[] => [])
    ])
    savedMethods.value = saved
    gatewayMethods.value = gateways
  }

  /**
   * Asks the backend to settle a hosted-gateway payment with the bank.
   *
   * A failure is never read as a decline: the money may well have moved, so it is reported as
   * {@link GatewayPaymentState.Unknown} and the reconciler settles the record regardless.
   *
   * @param target - Whether the payment was raised against an order or an invoice.
   * @param id - The order or invoice id.
   * @param paymentToken - The order's payment token; ignored for an invoice.
   * @returns What the bank said, or `Unknown` when no answer could be had.
   */
  const verifyGatewayPayment = async (
    target: PaymentTarget,
    id: string,
    paymentToken?: string
  ): Promise<GatewayPaymentState> => {
    try {
      const { state } = await billing.completeGatewayPayment(target, id, paymentToken)
      return STATE_BY_WIRE[state]
    } catch {
      return GatewayPaymentState.Unknown
    }
  }

  /** True while a payment request is in flight (and after a gateway hand-off, until the browser leaves). */
  const submitting = ref(false)

  /** True once a saved-method payment has been accepted. */
  const paySuccess = ref(false)

  /** The sentence to show for the last refused payment; empty when there is nothing to report. */
  const error = ref('')

  /**
   * Clears the state left by a previous invoice, called when the payment page opens.
   *
   * @returns Nothing.
   */
  const reset = (): void => {
    submitting.value = false
    paySuccess.value = false
    error.value = ''
  }

  /**
   * Clears only the last error, called before a new attempt that never reaches the API.
   *
   * @returns Nothing.
   */
  const clearError = (): void => {
    error.value = ''
  }

  /**
   * Charges the invoice to a saved payment method.
   *
   * @param invoiceId - Invoice to pay.
   * @param methodId - Id of the saved payment method.
   * @returns How the attempt ended; on failure {@link error} holds the sentence to show.
   */
  const payWithSavedMethod = async (invoiceId: string, methodId: string): Promise<ActionOutcome> => {
    error.value = ''
    submitting.value = true
    try {
      await billing.payInvoice(invoiceId, methodId)
      paySuccess.value = true
      return ActionOutcome.Succeeded
    } catch (e: unknown) {
      error.value = apiErrorMessage(e)
      return ActionOutcome.Failed
    } finally {
      submitting.value = false
    }
  }

  /**
   * Starts a hosted-gateway payment and returns where the browser must go next.
   *
   * On success {@link submitting} stays true: the page navigates away, and re-enabling the
   * buttons for that moment would invite a second payment.
   *
   * @param invoiceId - Invoice to pay.
   * @param module - Gateway plugin module id.
   * @param returnUrl - Absolute URL the gateway sends the customer back to.
   * @returns The gateway address to navigate to, or null when the attempt failed
   * ({@link error} holds the sentence to show).
   */
  const startGatewayPayment = async (
    invoiceId: string,
    module: string,
    returnUrl: string
  ): Promise<string | null> => {
    error.value = ''
    submitting.value = true
    try {
      const { redirectUrl } = await billing.startInvoiceGatewayPayment(invoiceId, module, returnUrl)
      return redirectUrl
    } catch (e: unknown) {
      error.value = apiErrorMessage(e)
      submitting.value = false
      return null
    }
  }

  /**
   * Builds the address of an invoice's PDF download for the page to link to.
   *
   * @param id - Invoice id.
   * @returns The address.
   */
  const invoicePdfUrl = (id: string): string => billing.invoicePdfUrl(id)

  return {
    invoice,
    invoiceLoading,
    invoicePdfUrl,
    invoiceError,
    savedMethods,
    gatewayMethods,
    hostedGatewayModule,
    loadInvoice,
    loadPaymentOptions,
    verifyGatewayPayment,
    submitting,
    paySuccess,
    error,
    reset,
    clearError,
    payWithSavedMethod,
    startGatewayPayment
  }
})

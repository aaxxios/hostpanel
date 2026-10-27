/**
 * Endpoints for money — invoices under `/api/portal/client/invoices`, the saved payment
 * methods at `/api/portal/client/payment-methods`, and ordering under `/api/portal/order`.
 *
 * Every function returns a plain `Promise`. Nothing here is cached or held.
 *
 * @module composables/apis/useBillingApi
 */

import { apiFetch } from '~/composables/useApi'
import type { ClientInvoice } from '~/types/clientinvoice'
import type { GatewayMethod } from '~/types/gatewaymethod'
import type { PaymentMethod, PaymentMethodWire } from '~/types/payment'
import { PaymentTarget } from '~/types/paymenttarget'
import { toPaymentMethod } from '~/utils/paymentMethodMapper'

/**
 * The billing surface, one function per endpoint.
 *
 * @returns The billing endpoint functions.
 */
export const useBillingApi = () => {
  /**
   * Reads one invoice with its line items.
   *
   * @param id - Invoice id.
   * @returns The invoice.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchInvoice = (id: string): Promise<ClientInvoice> =>
    apiFetch<ClientInvoice>(`/api/portal/client/invoices/${id}`)

  /**
   * Reads the client's saved cards and bank accounts as a one-shot call.
   *
   * @returns Every saved payment method on the account, mapped from the API's camelCase to {@link PaymentMethod}.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchPaymentMethods = async (): Promise<PaymentMethod[]> => {
    const wire = await apiFetch<PaymentMethodWire[]>('/api/portal/client/payment-methods')
    return Array.isArray(wire) ? wire.map(toPaymentMethod) : []
  }

  /**
   * Reads the gateways an order may be paid through.
   *
   * Note the path: this is under `/api/portal/order`, not `/api/portal/client`, because the
   * list is the same whether or not anybody is signed in.
   *
   * @returns Every gateway the order can be paid through.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchGatewayMethods = (): Promise<GatewayMethod[]> =>
    apiFetch<GatewayMethod[]>('/api/portal/order/payment-methods')

  /**
   * Pays an invoice with a saved payment method.
   *
   * @param invoiceId - Invoice to pay.
   * @param payMethodId - Saved payment method to charge.
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws — a declined card included.
   */
  const payInvoice = (invoiceId: string, payMethodId: string | number): Promise<unknown> =>
    apiFetch(`/api/portal/client/invoices/${invoiceId}/pay`, {
      method: 'POST',
      body: { paymethodid: payMethodId }
    })

  /**
   * Updates a saved payment method's details.
   *
   * @param id - Payment method to update.
   * @param body - The fields to change.
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws.
   */
  const updatePaymentMethod = (id: string, body: Record<string, unknown>): Promise<unknown> =>
    apiFetch(`/api/portal/client/payment-methods/${id}`, { method: 'PUT', body })

  /**
   * Makes a saved payment method the account default.
   *
   * @param id - Payment method to promote.
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws.
   */
  const setDefaultPaymentMethod = (id: string): Promise<unknown> =>
    apiFetch(`/api/portal/client/payment-methods/${id}`, {
      method: 'PUT',
      body: { set_as_default: true }
    })

  /**
   * Removes a saved payment method.
   *
   * @param id - Payment method to delete.
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws.
   */
  const removePaymentMethod = (id: string): Promise<unknown> =>
    apiFetch(`/api/portal/client/payment-methods/${id}`, { method: 'DELETE' })

  /**
   * Places an order, creating the order and its invoice on the backend.
   *
   * @param order - Cart contents, billing details and chosen payment method.
   * @returns The new order and invoice ids, plus the payment token that authorises paying for the order.
   * @throws Whatever `apiFetch` throws.
   */
  const createOrder = (order: Record<string, unknown>): Promise<{ orderId: number, invoiceId: number, paymentToken: string }> =>
    apiFetch<{ orderId: number, invoiceId: number, paymentToken: string }>('/api/portal/order/create', {
      method: 'POST',
      body: order
    })

  /**
   * Opens a Stripe PaymentIntent for an order.
   *
   * @param orderId - Order to charge.
   * @param paymentToken - The order's payment token, returned when it was placed.
   * @returns The client secret Stripe.js needs to confirm the card.
   * @throws Whatever `apiFetch` throws.
   */
  const createOrderPaymentIntent = (
    orderId: string | number,
    paymentToken: string
  ): Promise<{ clientSecret: string }> =>
    apiFetch<{ clientSecret: string }>(`/api/portal/order/${orderId}/create-payment-intent`, {
      method: 'POST',
      body: { paymentToken }
    })

  /**
   * Registers a hosted-gateway payment for an order and asks where to send the payer.
   *
   * @param orderId - Order to pay.
   * @param module - Gateway module to pay through.
   * @param returnUrl - Absolute URL the bank returns the payer to.
   * @param paymentToken - The order's payment token, returned when it was placed. Checkout is open
   * to guests, so this is what authorises paying for an order, in place of a credential.
   * @returns The bank's URL to hand the browser to.
   * @throws Whatever `apiFetch` throws.
   */
  const startOrderGatewayPayment = (
    orderId: string | number,
    module: string,
    returnUrl: string,
    paymentToken: string
  ): Promise<{ redirectUrl: string }> =>
    apiFetch<{ redirectUrl: string }>(`/api/portal/order/${orderId}/gateway-payment/start`, {
      method: 'POST',
      body: { module, returnUrl, paymentToken }
    })

  /**
   * Registers a hosted-gateway payment for an invoice and asks where to send the payer.
   *
   * @param invoiceId - Invoice to pay.
   * @param module - Gateway module to pay through.
   * @param returnUrl - Absolute URL the bank returns the payer to.
   * @returns The bank's URL to hand the browser to.
   * @throws Whatever `apiFetch` throws.
   */
  const startInvoiceGatewayPayment = (
    invoiceId: string | number,
    module: string,
    returnUrl: string
  ): Promise<{ redirectUrl: string }> =>
    apiFetch<{ redirectUrl: string }>(
      `/api/portal/client/invoices/${invoiceId}/gateway-payment/start`,
      { method: 'POST', body: { module, returnUrl } }
    )

  /**
   * Asks the backend to settle a hosted-gateway payment with the bank, after the payer has
   * been returned to the site.
   *
   * @param target - Whether the payment was raised against an order or an invoice.
   * @param id - The order or invoice id.
   * @param paymentToken - The order's payment token. Required when `target` is `PaymentTarget.Order`; ignored
   * for an invoice, which is authorised by the caller's own credential.
   * @returns What the bank said the payment did.
   * @throws Whatever `apiFetch` throws — the caller must not read a failure here as a
   * declined payment; the money may well have moved.
   */
  const completeGatewayPayment = (
    target: PaymentTarget,
    id: string | number,
    paymentToken?: string
  ): Promise<{ state: 'paid' | 'pending' | 'declined' }> =>
    apiFetch<{ state: 'paid' | 'pending' | 'declined' }>(
      target === PaymentTarget.Order
        ? `/api/portal/order/${id}/gateway-payment/complete`
        : `/api/portal/client/invoices/${id}/gateway-payment/complete`,
      // Only the order endpoint reads a token; the invoice one is behind the client's own
      // credential and is sent an empty body, exactly as before.
      { method: 'POST', body: target === PaymentTarget.Order ? { paymentToken } : {} }
    )

  /**
   * Confirms a card payment against an order after the gateway has authorised it.
   *
   * @param orderId - Order the payment belongs to.
   * @param body - Whatever the gateway handed back for confirmation.
   * @param paymentToken - The order's payment token, returned when it was placed.
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws.
   */
  const confirmOrderPayment = (
    orderId: string | number,
    body: Record<string, unknown>,
    paymentToken: string
  ): Promise<unknown> =>
    apiFetch(`/api/portal/order/${orderId}/confirm-payment`, {
      method: 'POST',
      body: { ...body, paymentToken }
    })

  /**
   * Reads an invoice as a PDF.
   *
   * No `responseType` is set: ofetch picks it from the answer, so the PDF arrives as a `Blob`
   * while a refusal still arrives as parsed JSON and `apiErrorMessage` can read its sentence.
   *
   * @param id - Invoice id.
   * @returns The PDF.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchInvoicePdf = (id: string): Promise<Blob> =>
    apiFetch<Blob>(`/api/portal/client/invoices/${id}/pdf`)

  return {
    fetchInvoice,
    fetchInvoicePdf,
    fetchPaymentMethods,
    fetchGatewayMethods,
    createOrder,
    createOrderPaymentIntent,
    startOrderGatewayPayment,
    startInvoiceGatewayPayment,
    completeGatewayPayment,
    payInvoice,
    updatePaymentMethod,
    setDefaultPaymentMethod,
    removePaymentMethod,
    confirmOrderPayment
  }
}

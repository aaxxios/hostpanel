/**
 * Tests for `composables/apis/useBillingApi.ts`.
 *
 * Each endpoint function is called for real through `apiFetch` against a stub server, and the
 * test asserts on the path, verb, query and body that arrived and on how the reply was typed.
 *
 * @module tests/composables/apis/useBillingApi.test
 */

import { afterEach, describe, expect, it } from 'vitest'
import { expectLastRequest, startTransport } from '../../helpers/stubTransport'
import type { ExpectedRequest } from '../../helpers/stubTransport'
import type { StubServer } from '../../helpers/stubServer'
import { useBillingApi } from '~/composables/apis/useBillingApi'
import { PaymentTarget } from '~/types/paymenttarget'

/** The stub started by the current test. */
let stub: StubServer | undefined

afterEach(async () => {
  await stub?.close()
  stub = undefined
})

/** The api surface under test. */
const api = useBillingApi()

/** Every endpoint function with the request it must send. */
const rows: ExpectedRequest[] = [
  { name: 'fetchInvoice reads one invoice', call: () => api.fetchInvoice('7'), method: 'GET', url: '/api/portal/client/invoices/7' },
  { name: 'fetchPaymentMethods reads the saved methods', call: () => api.fetchPaymentMethods(), method: 'GET', url: '/api/portal/client/payment-methods' },
  { name: 'fetchGatewayMethods reads the gateways under the order surface', call: () => api.fetchGatewayMethods(), method: 'GET', url: '/api/portal/order/payment-methods' },
  { name: 'payInvoice posts the saved method id', call: () => api.payInvoice('7', 'pm_1'), method: 'POST', url: '/api/portal/client/invoices/7/pay', body: { paymethodid: 'pm_1' } },
  { name: 'updatePaymentMethod puts the changed fields', call: () => api.updatePaymentMethod('pm_1', { card_expiry: '12 / 30' }), method: 'PUT', url: '/api/portal/client/payment-methods/pm_1', body: { card_expiry: '12 / 30' } },
  { name: 'setDefaultPaymentMethod puts the default flag', call: () => api.setDefaultPaymentMethod('pm_1'), method: 'PUT', url: '/api/portal/client/payment-methods/pm_1', body: { set_as_default: true } },
  { name: 'removePaymentMethod deletes by id', call: () => api.removePaymentMethod('pm_1'), method: 'DELETE', url: '/api/portal/client/payment-methods/pm_1' },
  { name: 'createOrder posts the order', call: () => api.createOrder({ items: [], paymentmethod: 'stripe' }), method: 'POST', url: '/api/portal/order/create', body: { items: [], paymentmethod: 'stripe' } },
  { name: 'createOrderPaymentIntent posts the payment token', call: () => api.createOrderPaymentIntent(42, 'tok'), method: 'POST', url: '/api/portal/order/42/create-payment-intent', body: { paymentToken: 'tok' } },
  { name: 'startOrderGatewayPayment posts module, return address and token', call: () => api.startOrderGatewayPayment(42, 'inecobank', 'https://x/return', 'tok'), method: 'POST', url: '/api/portal/order/42/gateway-payment/start', body: { module: 'inecobank', returnUrl: 'https://x/return', paymentToken: 'tok' } },
  { name: 'startInvoiceGatewayPayment posts module and return address', call: () => api.startInvoiceGatewayPayment(7, 'inecobank', 'https://x/return'), method: 'POST', url: '/api/portal/client/invoices/7/gateway-payment/start', body: { module: 'inecobank', returnUrl: 'https://x/return' } },
  { name: 'completeGatewayPayment for an order sends the token', call: () => api.completeGatewayPayment(PaymentTarget.Order, 42, 'tok'), method: 'POST', url: '/api/portal/order/42/gateway-payment/complete', body: { paymentToken: 'tok' } },
  { name: 'completeGatewayPayment for an invoice sends an empty body', call: () => api.completeGatewayPayment(PaymentTarget.Invoice, 7), method: 'POST', url: '/api/portal/client/invoices/7/gateway-payment/complete', body: {} },
  { name: 'confirmOrderPayment merges the token into the body', call: () => api.confirmOrderPayment(42, { paymentIntentId: 'pi_1' }, 'tok'), method: 'POST', url: '/api/portal/order/42/confirm-payment', body: { paymentIntentId: 'pi_1', paymentToken: 'tok' } },
]

describe('useBillingApi', () => {
  it.each(rows)('$name', async (row) => {
    stub = await startTransport()

    await row.call()

    expectLastRequest(stub, row)
  })

  it('returns the order ids the endpoint answers with', async () => {
    stub = await startTransport(() => ({ body: { orderId: 42, invoiceId: 77, paymentToken: 'tok' } }))

    const placed = await api.createOrder({})

    expect(placed).toEqual({ orderId: 42, invoiceId: 77, paymentToken: 'tok' })
  })
})

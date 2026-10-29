/**
 * What the API answers a placed order with.
 *
 * @module types/placeorderresult
 */

/** What the API answers a placed order with. */
export interface PlaceOrderResult {
  /** The new order's id. */
  orderId: number
  /** The order number the backend assigned, shown to the payer as-is. */
  orderNumber: string
  /** The order's invoice id; 0 when no invoice was raised. */
  invoiceId: number
  /** The token that authorises paying for this order. */
  paymentToken: string
  /** What the backend billed for the order, in {@link currency}; compared with the total the cart showed. */
  total: number
  /** ISO 4217 code of the currency the order is billed in. */
  currency: string
}

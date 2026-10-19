/**
 * What a hosted-gateway payment pays for.
 *
 * @module types/paymenttarget
 */
export enum PaymentTarget {
  /** A just-placed order, paid without an account. */
  Order = 'order',
  /** An invoice of a signed-in customer. */
  Invoice = 'invoice'
}

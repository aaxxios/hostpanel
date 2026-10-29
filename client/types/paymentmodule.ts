/**
 * Module ids of the payment methods the portal knows by name.
 *
 * `Stripe` and `BankTransfer` are built into checkout and mirror the backend's built-in module
 * ids, since the API returns these exact strings in the `module` field of
 * `GET /api/payment-methods`. `Inecobank` is a gateway plugin rather than a built-in; it is known
 * here only to pick its icon. Any other plugin id stays a plain string on the wire.
 */
export enum PaymentModule {
  /** The built-in card payment method. */
  Stripe = 'stripe',
  /** The built-in manual bank transfer payment method. */
  BankTransfer = 'bank_transfer',
  /** The Inecobank card gateway plugin. */
  Inecobank = 'inecobank'
}

/**
 * Saved payment method - a card or bank account on file for the authenticated client
 * Used in the client account page's Payment Methods tab
 * Returned by GET /api/portal/client/payment-methods (proxying the C# backend's
 * api/me/payment-methods) as {@link PaymentMethodWire}, then mapped to {@link PaymentMethod}
 * by `toPaymentMethod`.
 */

/** The kinds of saved payment method the card gateway reports; a closed set, so an enum. */
export enum PaymentMethodType {
  /** A debit or credit card. */
  Card = 'card',
  /** A bank account (the gateway's `us_bank_account`). */
  BankAccount = 'us_bank_account'
}

/** One saved method exactly as the API sends it (camelCase). */
export interface PaymentMethodWire {
  /** Gateway payment method ID (e.g. "pm_1AbC...") */
  id: string
  /** The gateway's payment method type, a {@link PaymentMethodType} value */
  type: string
  /** Card network (e.g. "visa"), or the bank name for a bank account */
  brand?: string | null
  /** Last four digits of the card or account number */
  last4?: string | null
  /** Card expiry month (1-12); absent for a bank account */
  expiryMonth?: number | null
  /** Card expiry year (4 digits); absent for a bank account */
  expiryYear?: number | null
  /** Whether this is the client's default method */
  isDefault: boolean
}

/** One saved method as the client renders it. */
export interface PaymentMethod {
  /** Gateway payment method ID (e.g. "pm_1AbC...") */
  id: string
  /** The gateway's payment method type, a {@link PaymentMethodType} value */
  type: string
  /** Display label, e.g. "Visa •••• 4242"; falls back to gateway_name when absent */
  description: string
  /** Gateway name shown when there is no description */
  gateway_name: string
  /** Legacy sub-contact this card's billing address is assigned to, if any */
  contact_id?: number
  /** Last four digits of the card or account number */
  card_last_four?: string | null
  /** Formatted card expiry (MM/YY) */
  card_expiry?: string | null
  /** Card network (e.g. "visa") */
  card_type?: string | null
  /** Bank name, for a bank account payment method */
  bank_name?: string | null
  /** Whether this is the client's default method */
  is_default?: boolean
}

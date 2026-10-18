/** Closed set of payment gateways an invoice or transaction can name, as the API sends them. */
export const PaymentGateway = {
  /** No gateway chosen. */
  None: 'None',
  /** Paid by bank transfer. */
  BankTransfer: 'BankTransfer',
  /** Paid through Stripe. */
  Stripe: 'Stripe',
  /** Paid through PayPal. */
  PayPal: 'PayPal',
} as const

/** One of the payment gateways. */
export type PaymentGateway = (typeof PaymentGateway)[keyof typeof PaymentGateway]

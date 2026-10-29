/**
 * Remembers the payment token an order was placed with, so the payer can still prove the order
 * is theirs after the bank has sent them back.
 *
 * A guest checkout has no account, so the order's payment endpoints are authorised by this token
 * rather than by a credential. The card-form path never needs storage: the token is used a few
 * lines after it arrives. The hosted-gateway path does: the browser leaves for the bank and comes
 * back to `/payment/result` as a fresh navigation, with nothing but `?order=` in hand.
 *
 * The token lives in `sessionStorage`, not `localStorage`: the payer leaves for the bank and
 * returns in the same tab, which keeps its session storage, while a shared computer does not keep
 * a credential for an order after the tab is closed. An entry also expires after
 * {@link PAYMENT_TOKEN_TTL_MS}, and the result page drops it as soon as the payment reaches a
 * final state. A cookie set by the BFF would hide it from scripts altogether, but the token is
 * returned to the page by the order endpoint and sent back in request bodies by the existing
 * routes, so moving it is a change of those routes' contract and is left as it is.
 *
 * @module composables/useOrderPaymentToken
 */

/** Prefix for the per-order storage keys. */
const STORAGE_PREFIX = 'order-payment-token:'

/** How long a remembered token stays usable: longer than a bank session, short enough not to linger. */
export const PAYMENT_TOKEN_TTL_MS = 60 * 60 * 1000

/** What is kept for one order while the payer is at the bank. */
interface StoredPaymentToken {
  /** The payment token returned when the order was placed. */
  token: string
  /** The invoice the order is billed on, so a declined payment can be retried against it. */
  invoiceId: number
  /** Epoch milliseconds after which the entry is no longer used. */
  expiresAt: number
}

/**
 * Builds the storage key for one order.
 *
 * @param orderId - The order the token belongs to.
 * @returns The storage key.
 */
const keyFor = (orderId: string | number): string => `${STORAGE_PREFIX}${orderId}`

/**
 * True when a parsed storage value has the {@link StoredPaymentToken} shape.
 *
 * @param value - The parsed storage value.
 * @returns True when the value can be used as an entry.
 */
const isStored = (value: unknown): value is StoredPaymentToken => {
  if (typeof value !== 'object' || value === null) {
    return false
  }
  const entry = value as Record<string, unknown>
  return typeof entry.token === 'string' &&
    typeof entry.invoiceId === 'number' &&
    typeof entry.expiresAt === 'number'
}

/**
 * Reads, writes and clears the payment token for an order.
 *
 * Every function is a no-op on the server, where storage does not exist; `recall` returns
 * `undefined` there, which the callers already treat as "no token" and refuse on.
 *
 * @returns The remember/recall/recallInvoiceId/forget functions.
 */
export const useOrderPaymentToken = () => {
  /**
   * Reads the live entry of an order, dropping it when it has expired or is malformed.
   *
   * @param orderId - The order to look up.
   * @returns The entry, or `undefined` when none is stored or it is no longer valid.
   */
  const read = (orderId: string | number): StoredPaymentToken | undefined => {
    if (!import.meta.client) {
      return undefined
    }
    const raw = sessionStorage.getItem(keyFor(orderId))
    if (raw === null) {
      return undefined
    }
    try {
      const parsed: unknown = JSON.parse(raw)
      if (isStored(parsed) && parsed.expiresAt > Date.now()) {
        return parsed
      }
    } catch {
      // A malformed entry is dropped below like an expired one.
    }
    sessionStorage.removeItem(keyFor(orderId))
    return undefined
  }

  /**
   * Stores the token for an order.
   *
   * @param orderId - The order the token belongs to.
   * @param token - The payment token returned when the order was placed.
   * @param invoiceId - The invoice the order is billed on.
   */
  const remember = (orderId: string | number, token: string, invoiceId: number): void => {
    if (!import.meta.client) {
      return
    }
    const entry: StoredPaymentToken = { token, invoiceId, expiresAt: Date.now() + PAYMENT_TOKEN_TTL_MS }
    sessionStorage.setItem(keyFor(orderId), JSON.stringify(entry))
  }

  /**
   * Reads the stored token for an order.
   *
   * @param orderId - The order to look up.
   * @returns The token, or `undefined` when none was stored or it has expired.
   */
  const recall = (orderId: string | number): string | undefined => read(orderId)?.token

  /**
   * Reads the invoice id stored beside an order's token.
   *
   * @param orderId - The order to look up.
   * @returns The invoice id, or `undefined` when none was stored or it has expired.
   */
  const recallInvoiceId = (orderId: string | number): number | undefined => read(orderId)?.invoiceId

  /**
   * Drops the stored token for an order.
   *
   * @param orderId - The order whose token is no longer needed.
   */
  const forget = (orderId: string | number): void => {
    if (!import.meta.client) {
      return
    }
    sessionStorage.removeItem(keyFor(orderId))
  }

  return { remember, recall, recallInvoiceId, forget }
}

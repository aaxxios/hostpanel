/**
 * The mail categories a client can switch on or off.
 *
 * @module types/emailpreference
 */

/** One email category; the value is the key in the profile's `notifications`. */
export enum EmailPreference {
  /** General announcements. */
  General = 'general',
  /** Invoices and payment notices. */
  Invoice = 'invoice',
  /** Support ticket replies. */
  Support = 'support',
  /** Product and service notices. */
  Product = 'product',
  /** Domain notices. */
  Domain = 'domain',
  /** Affiliate notices. */
  Affiliate = 'affiliate'
}

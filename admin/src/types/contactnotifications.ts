/**
 * Which kinds of email a client or one of its contacts receives, as the API groups the flags.
 */

/** The six email kinds an account or contact can switch on or off. */
export interface ContactNotifications {
  /** General account emails. */
  general: boolean
  /** Invoice and billing emails. */
  invoice: boolean
  /** Support ticket emails. */
  support: boolean
  /** Product lifecycle emails. */
  product: boolean
  /** Domain registration emails. */
  domain: boolean
  /** Affiliate program emails. */
  affiliate: boolean
}

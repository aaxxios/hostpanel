/**
 * One email forwarding rule on a client's domain, as the API sends it inside `DomainManagement`.
 *
 * @module types/emailforwardingrule
 */

/** One email forwarding rule of a domain. */
export interface EmailForwardingRule {
  /** Rule primary key. */
  id: number
  /** Source alias or local part. */
  source: string
  /** Destination email address. */
  destination: string
  /** Whether the rule is currently active. */
  isActive: boolean
}

/**
 * Lifecycle statuses of a domain, as the API names them on
 * `ClientDomain.status`.
 */
export enum DomainStatus {
  /** Registered and live. */
  Active = 'Active',
  /** Past its expiry date. */
  Expired = 'Expired',
  /** Ordered, the registrar has not finished registering it. */
  PendingRegistration = 'PendingRegistration',
  /** A transfer in is under way. */
  PendingTransfer = 'PendingTransfer',
  /** In the registrar redemption grace period. */
  Redemption = 'Redemption',
  /** Transferred out to another registrar. */
  Transferred = 'Transferred',
  /** Cancelled before activation. */
  Cancelled = 'Cancelled'
}

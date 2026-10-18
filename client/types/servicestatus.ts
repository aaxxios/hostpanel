/**
 * Lifecycle statuses of a hosting service the portal branches on, as the API names them on
 * `ClientService.status`.
 */
export enum ServiceStatus {
  /** Provisioned and running. */
  Active = 'Active',
  /** Ordered, waiting for setup or provisioning. */
  Pending = 'Pending',
  /** Switched off, usually for non-payment. */
  Suspended = 'Suspended',
  /** Removed by staff. */
  Terminated = 'Terminated',
  /** Cancelled by the client. */
  Cancelled = 'Cancelled'
}

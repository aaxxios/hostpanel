/**
 * Why a hosting service is suspended, as the API names it on `ClientService.suspensionSource`
 * (the backend's `SuspensionSource`).
 *
 * @module types/suspensionsource
 */

/** Why a hosting service is suspended. */
export enum SuspensionSource {
  /** Suspended by staff. */
  Manual = 'Manual',
  /** Suspended because an invoice is overdue. */
  Overdue = 'Overdue',
  /** Suspended because the service's domain has expired. */
  DomainExpired = 'DomainExpired',
  /** Suspended for another reason. */
  Other = 'Other'
}

/**
 * The email preferences on the client's own profile, as `MyProfileNotificationsDto` sends them.
 *
 * @module types/myprofilenotifications
 */

/** The email preferences on the client's own profile. */
export interface MyProfileNotifications {
  /** General account emails enabled. */
  general: boolean
  /** Invoice emails enabled. */
  invoice: boolean
  /** Support emails enabled. */
  support: boolean
  /** Product emails enabled. */
  product: boolean
  /** Domain emails enabled. */
  domain: boolean
  /** Affiliate emails enabled. */
  affiliate: boolean
  /** Marketing emails opt-in. */
  marketingOptIn: boolean
  /** Status update emails sent. */
  statusUpdate: boolean
}

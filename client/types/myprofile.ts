/**
 * The client's own profile as `GET /api/clients/me` sends it (`MyProfileDto`). It carries no
 * staff-owned field, so nothing here can show the client what only staff should see.
 *
 * @module types/myprofile
 */

import type { AccountContact } from '~/types/accountcontact'
import type { PostalAddress } from '~/types/postaladdress'
import type { MyProfileNotifications } from '~/types/myprofilenotifications'

/** The client's own profile as the backend sends it. */
export interface MyProfile {
  /** Client primary key. */
  id: number
  /** Email address from the linked account. */
  email: string
  /** Client first name. */
  firstName: string
  /** Client last name. */
  lastName: string
  /** Company name, null for individuals. */
  companyName: string | null
  /** Phone number. */
  phone: string | null
  /** Billing address. */
  address: PostalAddress
  /** The account holder's chosen UI language, null where the deployment holds none. */
  language: string | null
  /** ISO 4217 currency code. */
  currency: string | null
  /** Preferred payment method. */
  paymentMethod: string | null
  /** Which emails the client receives. */
  notifications: MyProfileNotifications
  /** True if the user has TOTP 2FA enabled. */
  twoFactorEnabled: boolean
  /** Additional contacts linked to this client. */
  contacts: AccountContact[]
}

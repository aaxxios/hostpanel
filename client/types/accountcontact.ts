/**
 * One sub-contact on the client account, as the contacts endpoint lists it.
 *
 * @module types/accountcontact
 */

import type { ContactNotifications } from '~/types/contactnotifications'
import type { ContactType } from '~/types/contacttype'
import type { PostalAddress } from '~/types/postaladdress'

/**
 * One sub-contact on the client account, as `GET /api/portal/client/contacts` lists it. Mirrors
 * the backend `ContactDto` one to one (the BFF forwards it unmapped).
 */
export interface AccountContact {
  /** Contact id. */
  id: number
  /** Given name. */
  firstName: string
  /** Family name. */
  lastName: string
  /** Company, if any. */
  companyName: string | null
  /** Email address. */
  email: string
  /** Phone number, if any. */
  phone: string | null
  /** Classification of the contact. */
  type: ContactType
  /** The contact's postal address. */
  address: PostalAddress
  /** Which kinds of email the contact receives. */
  notifications: ContactNotifications
}

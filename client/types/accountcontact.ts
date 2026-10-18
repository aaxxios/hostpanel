/**
 * One sub-contact on the client account, as the contacts endpoint lists it.
 *
 * @module types/accountcontact
 */

import type { ContactType } from '~/types/contacttype'

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
  /** Street address, if any. */
  street: string | null
  /** Second address line, if any. */
  address2: string | null
  /** City, if any. */
  city: string | null
  /** State or region, if any. */
  state: string | null
  /** Postal code, if any. */
  postCode: string | null
  /** ISO 3166-1 alpha-2 country code, if any. */
  country: string | null
  /** Whether the contact receives general emails. */
  notifyGeneral: boolean
  /** Whether the contact receives invoice emails. */
  notifyInvoice: boolean
  /** Whether the contact receives support emails. */
  notifySupport: boolean
  /** Whether the contact receives product emails. */
  notifyProduct: boolean
  /** Whether the contact receives domain emails. */
  notifyDomain: boolean
  /** Whether the contact receives affiliate emails. */
  notifyAffiliate: boolean
}

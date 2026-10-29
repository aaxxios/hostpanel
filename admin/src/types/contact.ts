
import type { ContactNotifications } from '@/types/contactnotifications'
import type { PostalAddress } from '@/types/postaladdress'

/** Contact record linked to a client account. */
export interface Contact {
  /** Contact primary key. */
  id: number
  /** First name. */
  firstName: string
  /** Last name. */
  lastName: string
  /** Optional company name. */
  companyName?: string
  /** Email address. */
  email: string
  /** Phone number. */
  phone?: string
  /** Contact type: Billing, Technical, or General. */
  type: string
  /** Postal address. */
  address: PostalAddress
  /** Which kinds of email the contact receives. */
  notifications: ContactNotifications
  /** ISO 8601 creation timestamp. */
  createdAt: string
}

/** Kind of a client contact, exactly as the API sends it. */
export enum ContactType {
  Billing = 'Billing',
  Technical = 'Technical',
  General = 'General',
}

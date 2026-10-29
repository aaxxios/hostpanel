/**
 * Factory for account contacts in the backend's `ContactDto` shape, so the account tests build
 * the same wire object the API sends instead of each spelling out nineteen fields.
 *
 * @module tests/helpers/accountContact
 */

import type { AccountContact } from '~/types/accountcontact'
import type { AccountContactInput } from '~/types/accountcontactinput'
import { ContactType } from '~/types/contacttype'

/**
 * Builds a contact with every optional field unset.
 *
 * @param overrides - Fields to set on top of the defaults.
 * @returns The contact.
 */
export const makeContact = (overrides: Partial<AccountContact> = {}): AccountContact => ({
  id: 1,
  firstName: 'Ann',
  lastName: 'Lee',
  companyName: null,
  email: 'ann@example.com',
  phone: null,
  type: ContactType.General,
  address: { street: null, address2: null, city: null, state: null, postCode: null, country: null },
  notifications: { general: true, invoice: true, support: true, product: true, domain: false, affiliate: false },
  ...overrides
})

/**
 * Builds a contact request body (a contact without its id).
 *
 * @param overrides - Fields to set on top of the defaults.
 * @returns The request body.
 */
export const makeContactInput = (overrides: Partial<AccountContactInput> = {}): AccountContactInput => {
  const { id: _id, ...input } = makeContact()
  return { ...input, ...overrides }
}

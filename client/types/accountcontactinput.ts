/**
 * The body of a contact create or update: the account contact without its id.
 *
 * @module types/accountcontactinput
 */

import type { AccountContact } from '~/types/accountcontact'

/**
 * The body of a contact create (`AddContactCommand`) or update (`UpdateContactCommand`): every
 * field of {@link AccountContact} except the id, which travels in the URL. The client id is never
 * sent; the backend takes it from the credential.
 */
export type AccountContactInput = Omit<AccountContact, 'id'>

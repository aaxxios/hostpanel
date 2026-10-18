/**
 * Mapping between the account's contacts and the WHOIS contact fields of a domain.
 *
 * The domain contact tab pre-fills the registrant contact from the account's primary profile or
 * from one of its other contacts. The profile comes from the client API in the BFF's own field
 * names (`firstname`, `phonenumber`...), the contacts in the backend `ContactDto` names
 * (`firstName`, `phone`, `postCode`...); the domain endpoint takes `DomainContactFields`. These
 * pure functions do the renaming so the panel only wires state.
 *
 * @module utils/domainContacts
 */

import type { AccountContact } from '~/types/accountcontact'
import type { DomainContactChoice } from '~/types/domaincontactchoice'
import type { DomainContactFields } from '~/types/domaincontactfields'
import type { RegistrantContact } from '~/types/registrantcontact'

/** The id the contact dropdown gives the account's primary profile. */
export const PRIMARY_CONTACT_ID = 'primary'

/**
 * Creates a contact with every field empty.
 *
 * @returns A fresh object, never shared between forms.
 */
export const emptyContact = (): DomainContactFields => ({
  firstName: '', lastName: '', organization: '', email: '',
  address1: '', address2: '', city: '', state: '',
  postalCode: '', country: '', phone: ''
})

/**
 * Maps the registrant contact the registrar holds to WHOIS contact fields.
 *
 * The two optional lines arrive as null; the form binds empty strings.
 *
 * @param contact - The registrant contact as the backend answers it.
 * @returns The contact fields.
 */
export const contactFromRegistrant = (contact: RegistrantContact): DomainContactFields => ({
  firstName: contact.firstName,
  lastName: contact.lastName,
  organization: contact.organization ?? '',
  email: contact.email,
  address1: contact.address1,
  address2: contact.address2 ?? '',
  city: contact.city,
  state: contact.state,
  postalCode: contact.postalCode,
  country: contact.country,
  phone: contact.phone
})

/**
 * Maps the account's primary profile to WHOIS contact fields.
 *
 * @param profile - The profile as the client API returns it; absent fields read as empty.
 * @returns The contact fields.
 */
export const contactFromProfile = (profile: Record<string, string>): DomainContactFields => ({
  firstName: profile.firstname || '',
  lastName: profile.lastname || '',
  organization: profile.companyname || '',
  email: profile.email || '',
  address1: profile.address1 || '',
  address2: profile.address2 || '',
  city: profile.city || '',
  state: profile.state || '',
  postalCode: profile.postcode || '',
  country: profile.country || '',
  phone: profile.phonenumber || ''
})

/**
 * Maps the account's contacts to choices of the "Choose contact" dropdown.
 *
 * A choice pre-fills every WHOIS field the contact carries; a field the contact has not set
 * (null on the wire) becomes an empty string, which is what the form binds.
 *
 * @param contacts - The contacts as the client API lists them.
 * @returns One choice per contact, labelled with the full name.
 */
export const toContactChoices = (contacts: AccountContact[]): DomainContactChoice[] =>
  contacts.map((contact) => ({
    id: String(contact.id),
    label: `${contact.firstName} ${contact.lastName}`.trim(),
    fields: {
      firstName: contact.firstName,
      lastName: contact.lastName,
      organization: contact.companyName ?? '',
      email: contact.email,
      address1: contact.street ?? '',
      address2: contact.address2 ?? '',
      city: contact.city ?? '',
      state: contact.state ?? '',
      postalCode: contact.postCode ?? '',
      country: contact.country ?? '',
      phone: contact.phone ?? ''
    }
  }))

/**
 * Tests for `utils/domainContacts.ts`: the field renaming and the dropdown choices.
 */
import { describe, expect, it } from 'vitest'
import type { AccountContact } from '~/types/accountcontact'
import { ContactType } from '~/types/contacttype'
import { contactFromProfile, contactFromRegistrant, emptyContact, toContactChoices } from '~/utils/domainContacts'

/**
 * Builds a contact with every optional field unset.
 *
 * @param overrides - Fields to set.
 * @returns The contact.
 */
const contactWith = (overrides: Partial<AccountContact>): AccountContact => ({
  id: 1,
  firstName: '',
  lastName: '',
  companyName: null,
  email: '',
  phone: null,
  type: ContactType.General,
  street: null,
  address2: null,
  city: null,
  state: null,
  postCode: null,
  country: null,
  notifyGeneral: true,
  notifyInvoice: true,
  notifySupport: true,
  notifyProduct: true,
  notifyDomain: false,
  notifyAffiliate: false,
  ...overrides
})

describe('emptyContact', () => {
  it('has every field empty and is a new object each time', () => {
    const first = emptyContact()

    expect(Object.values(first).every((value) => value === '')).toBe(true)
    expect(emptyContact()).not.toBe(first)
  })
})

describe('contactFromProfile', () => {
  it('renames the profile fields to the WHOIS fields', () => {
    const contact = contactFromProfile({
      firstname: 'Ada', lastname: 'Lovelace', companyname: 'Engines', email: 'ada@example.com',
      address1: '1 Main St', address2: 'Flat 2', city: 'London', state: 'LDN',
      postcode: 'N1', country: 'GB', phonenumber: '+44 1'
    })

    expect(contact).toEqual({
      firstName: 'Ada', lastName: 'Lovelace', organization: 'Engines', email: 'ada@example.com',
      address1: '1 Main St', address2: 'Flat 2', city: 'London', state: 'LDN',
      postalCode: 'N1', country: 'GB', phone: '+44 1'
    })
  })

  it('reads missing fields as empty', () => {
    expect(contactFromProfile({ firstname: 'Ada' })).toEqual({ ...emptyContact(), firstName: 'Ada' })
  })
})

describe('contactFromRegistrant', () => {
  it('maps the registrar contact and reads the null optional lines as empty', () => {
    expect(contactFromRegistrant({
      firstName: 'Ada', lastName: 'Lovelace', organization: null, email: 'ada@example.com', phone: '+44 1',
      address1: '1 Main St', address2: null, city: 'London', state: 'LDN', postalCode: 'N1', country: 'GB'
    })).toEqual({
      firstName: 'Ada', lastName: 'Lovelace', organization: '', email: 'ada@example.com',
      address1: '1 Main St', address2: '', city: 'London', state: 'LDN',
      postalCode: 'N1', country: 'GB', phone: '+44 1'
    })
  })
})

describe('toContactChoices', () => {
  it('labels a choice with the full name and pre-fills every WHOIS field the contact carries', () => {
    const [choice] = toContactChoices([contactWith({
      id: 7, firstName: 'Ada', lastName: 'Lovelace', companyName: 'Engines', email: 'ada@example.com', phone: '+44 1',
      street: '1 Main St', city: 'London', postCode: 'N1', country: 'GB'
    })])

    expect(choice).toEqual({
      id: '7',
      label: 'Ada Lovelace',
      fields: {
        firstName: 'Ada', lastName: 'Lovelace', organization: 'Engines', email: 'ada@example.com',
        address1: '1 Main St', address2: '', city: 'London', state: '', postalCode: 'N1', country: 'GB', phone: '+44 1'
      }
    })
  })

  it('trims the label of a contact with one name', () => {
    expect(toContactChoices([contactWith({ firstName: 'Ada', lastName: '' })])[0]!.label).toBe('Ada')
  })
})

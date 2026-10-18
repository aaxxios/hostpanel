/** Contact fields matching the backend `ModifyContactRequest` shape, for one domain contact. */
export interface DomainContactFields {
  /** Registrant first name. */
  firstName: string
  /** Registrant last name. */
  lastName: string
  /** Organization name. */
  organization: string
  /** Contact email address. */
  email: string
  /** Street address line 1. */
  address1: string
  /** Street address line 2. */
  address2: string
  /** City. */
  city: string
  /** State or province. */
  state: string
  /** Postal/zip code. */
  postalCode: string
  /** ISO 3166-1 alpha-2 country code. */
  country: string
  /** Contact phone number. */
  phone: string
}

/**
 * A postal address as the API groups it on a client or a contact.
 */

/** Every line of a postal address; a line that is not set is left out. */
export interface PostalAddress {
  /** Street address. */
  street?: string | null
  /** Second address line. */
  address2?: string | null
  /** City. */
  city?: string | null
  /** State or region. */
  state?: string | null
  /** Postal code. */
  postCode?: string | null
  /** ISO 3166-1 alpha-2 country code. */
  country?: string | null
}

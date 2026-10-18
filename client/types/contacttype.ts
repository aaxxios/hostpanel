/**
 * The classification of a sub-contact on the client account, as the backend's `ContactType`
 * enum names it. The API serialises enum members by name, so the values are the wire strings.
 */
export enum ContactType {
  /** Receives billing and invoice emails. */
  Billing = 'Billing',
  /** Receives technical and service notifications. */
  Technical = 'Technical',
  /** General purpose contact. */
  General = 'General'
}

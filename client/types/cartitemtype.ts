/**
 * What a cart line is: a hosting plan or a domain.
 *
 * @module types/cartitemtype
 */
export enum CartItemType {
  /** A hosting plan; the default when a line carries no type. */
  Hosting = 'hosting',
  /** A domain registration or transfer. */
  Domain = 'domain'
}

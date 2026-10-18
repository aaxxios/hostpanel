/**
 * What the visitor did to a domain at checkout, as the order-success link carries it in `?action=`.
 *
 * @module types/domainorderaction
 */
export enum DomainOrderAction {
  /** A new registration. */
  Register = 'register',
  /** A transfer in from another registrar. */
  Transfer = 'transfer'
}

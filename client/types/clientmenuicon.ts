/**
 * The icons the single sign-on account widget can draw beside a quick link; the values are the
 * identifiers the widget switches on.
 *
 * @module types/clientmenuicon
 */
export enum ClientMenuIcon {
  /** The client dashboard. */
  Dashboard = 'dashboard',
  /** The client's services. */
  Services = 'services',
  /** The client's domains. */
  Domains = 'domains',
  /** The client's invoices. */
  Invoices = 'invoices',
  /** Support tickets. */
  Support = 'support'
}

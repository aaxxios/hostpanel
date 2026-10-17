/**
 * The permissions a team invitation can grant.
 *
 * @module types/teampermission
 */

/** One grantable permission; the value is the word the API stores. */
export enum TeamPermission {
  /** Edit the profile. */
  Profile = 'profile',
  /** Manage contacts. */
  Contacts = 'contacts',
  /** Manage products and services. */
  Products = 'products',
  /** View service passwords. */
  Passwords = 'passwords',
  /** Single sign-on to services. */
  Sso = 'sso',
  /** View domains. */
  Domains = 'domains',
  /** Change domain settings. */
  DomainSettings = 'domainsettings',
  /** View and pay invoices. */
  Invoices = 'invoices',
  /** View quotes. */
  Quotes = 'quotes',
  /** Open and answer tickets. */
  Tickets = 'tickets',
  /** Use the affiliate area. */
  Affiliate = 'affiliate',
  /** Read the sent-mail history. */
  Emails = 'emails',
  /** Place orders. */
  Orders = 'orders'
}

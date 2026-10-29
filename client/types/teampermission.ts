/**
 * The permissions a team invitation can grant.
 *
 * @module types/teampermission
 */

/** One grantable permission; the value is the API's permission name. */
export enum TeamPermission {
  /** Edit the profile. */
  Profile = 'ModifyMasterProfile',
  /** Manage contacts. */
  Contacts = 'ViewManageContacts',
  /** Manage products and services. */
  Products = 'ViewProductsServices',
  /** View service passwords. */
  Passwords = 'ViewModifyPasswords',
  /** Single sign-on to services. */
  Sso = 'AllowSingleSignOn',
  /** View domains. */
  Domains = 'ViewDomains',
  /** Change domain settings. */
  DomainSettings = 'ManageDomainSettings',
  /** View and pay invoices. */
  Invoices = 'ViewPayInvoices',
  /** View quotes. */
  Quotes = 'ViewAcceptQuotes',
  /** Open and answer tickets. */
  Tickets = 'ViewOpenSupportTickets',
  /** Use the affiliate area. */
  Affiliate = 'ViewManageAffiliate',
  /** Read the sent-mail history. */
  Emails = 'ViewEmails',
  /** Place orders. */
  Orders = 'PlaceNewOrders'
}

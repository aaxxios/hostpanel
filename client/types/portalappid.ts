/**
 * The sibling products the app launcher can offer; each value is also the name of the matching
 * `portal.apps.*` setting and of the `aurora.apps.items.<id>` locale entry.
 *
 * @module types/portalappid
 */
export enum PortalAppId {
  /** The account centre. */
  Account = 'account',
  /** The task tracker. */
  Tasks = 'tasks',
  /** The resource-planning product. */
  Erp = 'erp',
  /** This hosting portal. */
  Hostpanel = 'hostpanel',
  /** The spreadsheet product. */
  Sheets = 'sheets',
  /** The mail product. */
  Mail = 'mail',
  /** The documents product. */
  Docs = 'docs',
  /** The calendar product. */
  Calendar = 'calendar'
}

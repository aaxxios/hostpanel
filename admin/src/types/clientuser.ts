/**
 * A sub-account on a client, the permission flags it can hold, and their labels.
 * 
 * The labels sit beside the enum rather than in a component so every screen that renders a
 * permission spells it the same way.
 */

import { translate } from '@/lib/translate'


/** User linked to a client account with permissions. */
export interface ClientUserItem {
  /** Identity user ID. */
  userId: string
  /** First name. */
  firstName: string
  /** Last name. */
  lastName: string
  /** Email address. */
  email: string
  /** True if this user is the account owner. */
  isOwner: boolean
  /** Granted permissions as bit-flags integer. */
  permissions: number
  /** Last login timestamp or null. */
  lastLoginAt: string | null
  /** When the user was linked. */
  createdAt: string
}

/** Granular permissions for users linked to a client account. */
export enum ClientPermission {
  /** No permissions. */
  None = 0,
  /** Modify the master account profile. */
  ModifyMasterProfile = 1,
  /** View and manage contacts. */
  ViewManageContacts = 2,
  /** View products and services. */
  ViewProductsServices = 4,
  /** View and modify product passwords. */
  ViewModifyPasswords = 8,
  /** Allow single sign-on to hosting panels. */
  AllowSingleSignOn = 16,
  /** View domains. */
  ViewDomains = 32,
  /** Manage domain settings. */
  ManageDomainSettings = 64,
  /** View and pay invoices. */
  ViewPayInvoices = 128,
  /** View and accept quotes. */
  ViewAcceptQuotes = 256,
  /** View and open support tickets. */
  ViewOpenSupportTickets = 512,
  /** View and manage affiliate account. */
  ViewManageAffiliate = 1024,
  /** View emails. */
  ViewEmails = 2048,
  /** Place new orders, upgrades, and cancellations. */
  PlaceNewOrders = 4096,
  /** All permissions combined. */
  All = 8191,
}

/** Labels for each permission flag, used for checkbox rendering. */
export const PERMISSION_LABELS: { flag: ClientPermission; label: string }[] = [
  { flag: ClientPermission.ModifyMasterProfile, get label() { return translate('permissions.permissionLabels.modifyMasterAccountProfile') } },
  { flag: ClientPermission.ViewManageContacts, get label() { return translate('permissions.permissionLabels.viewManageContacts') } },
  { flag: ClientPermission.ViewProductsServices, get label() { return translate('permissions.permissionLabels.viewProductsServices') } },
  { flag: ClientPermission.ViewModifyPasswords, get label() { return translate('permissions.permissionLabels.viewModifyProductPasswords') } },
  { flag: ClientPermission.AllowSingleSignOn, get label() { return translate('shared.allowSingleSignOn') } },
  { flag: ClientPermission.ViewDomains, get label() { return translate('permissions.permissionLabels.viewDomains') } },
  { flag: ClientPermission.ManageDomainSettings, get label() { return translate('permissions.permissionLabels.manageDomainSettings') } },
  { flag: ClientPermission.ViewPayInvoices, get label() { return translate('permissions.permissionLabels.viewPayInvoices') } },
  { flag: ClientPermission.ViewAcceptQuotes, get label() { return translate('permissions.permissionLabels.viewAcceptQuotes') } },
  { flag: ClientPermission.ViewOpenSupportTickets, get label() { return translate('permissions.permissionLabels.viewOpenSupportTickets') } },
  { flag: ClientPermission.ViewManageAffiliate, get label() { return translate('permissions.permissionLabels.viewManageAffiliateAccount') } },
  { flag: ClientPermission.ViewEmails, get label() { return translate('permissions.permissionLabels.viewEmails') } },
  { flag: ClientPermission.PlaceNewOrders, get label() { return translate('permissions.permissionLabels.placeNewOrdersUpgradesCancellations') } },
]

/** An Identity user offered by the add-user search. */
export interface UserSearchHit {
  /** Identity user id. */
  id: string
  /** Email address. */
  email: string
  /** First name. */
  firstName: string
  /** Last name. */
  lastName: string
}

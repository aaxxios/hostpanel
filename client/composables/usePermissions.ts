/**
 * Permission flags matching the C# backend `ClientPermission` enum.
 *
 * Each value is a power-of-two bit flag. Combine with bitwise OR to represent
 * multiple permissions (e.g. `ViewDomains | ViewPayInvoices`).
 *
 * @module composables/usePermissions
 */

import { holdsPermission } from '~/utils/holdsPermission'

/** Permission bit flags of a client account. */
export enum Permission {
  /** No permissions granted. */
  None = 0,
  /** Can edit the primary client profile. */
  ModifyMasterProfile = 1,
  /** Can view and manage sub-contacts. */
  ViewManageContacts = 2,
  /** Can view products and services. */
  ViewProductsServices = 4,
  /** Can view and modify service passwords. */
  ViewModifyPasswords = 8,
  /** Can use single sign-on into services. */
  AllowSingleSignOn = 16,
  /** Can view domains. */
  ViewDomains = 32,
  /** Can manage domain settings (DNS, WHOIS, etc.). */
  ManageDomainSettings = 64,
  /** Can view and pay invoices. */
  ViewPayInvoices = 128,
  /** Can view and accept quotes. */
  ViewAcceptQuotes = 256,
  /** Can view and open support tickets. */
  ViewOpenSupportTickets = 512,
  /** Can view and manage affiliate account. */
  ViewManageAffiliate = 1024,
  /** Can view notification emails. */
  ViewEmails = 2048,
  /** Can place new orders. */
  PlaceNewOrders = 4096,
  /** All permissions: owner default. */
  All = 8191,
}

/**
 * Composable for checking client portal permissions.
 *
 * Reads the current user's permissions from {@link useClientStore} (the record the client area
 * loads on every page), falling back to {@link useAuthStore} for the moment right after sign-in.
 * Reading only the auth store left every gated link hidden after a reload without the `authed`
 * cookie (the demo, SSO), because that store is filled only by a cookie-guarded plugin.
 * Provides helpers to check individual or combined permission flags.
 *
 * @returns Permission checking utilities and the {@link Permission} constants.
 */
export const usePermissions = () => {
  const { user: authUser } = storeToRefs(useAuthStore())
  const { user: clientUser } = storeToRefs(useClientStore())

  /** The signed-in person: the client store's record first, the auth store's second. */
  const user = computed(() => clientUser.value ?? authUser.value)

  /**
   * Checks whether the current user has a specific permission.
   *
   * Returns `true` when the user's permission integer contains the given flag bit,
   * or when the user has {@link Permission.All}.
   *
   * @param flag - The permission bit flag to check (use {@link Permission} constants).
   * @returns True if the user has the permission.
   */
  const hasPermission = (flag: number): boolean => {
    const perms = user.value?.permissions ?? 0
    if (flag === 0) {
      return true
    }
    return (perms & flag) !== 0
  }

  /**
   * Checks whether the current user has ALL of the specified permissions.
   *
   * @param flags - One or more permission flags combined with bitwise OR.
   * @returns True if the user has every specified permission.
   */
  const hasAllPermissions = (flags: number): boolean => {
    return holdsPermission(user.value?.permissions, flags)
  }

  return { Permission, hasPermission, hasAllPermissions }
}

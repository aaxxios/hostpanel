/**
 * How an invitation's permissions are given.
 *
 * @module types/invitepermissionmode
 */

/** Whether an invitee receives every permission or only the ones ticked; values match the API enum. */
export enum InvitePermissionMode {
  /** Every permission; the permission list is sent empty. */
  All = 'All',
  /** Only the ticked permissions. */
  Choose = 'Choose'
}

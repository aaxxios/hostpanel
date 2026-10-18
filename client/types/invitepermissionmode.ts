/**
 * How an invitation's permissions are given.
 *
 * @module types/invitepermissionmode
 */

/** Whether an invitee receives every permission or only the ones ticked. */
export enum InvitePermissionMode {
  /** Every permission; sent to the API as the word `all`. */
  All = 'all',
  /** Only the ticked permissions. */
  Choose = 'choose'
}

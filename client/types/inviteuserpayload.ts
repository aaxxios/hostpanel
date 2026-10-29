/**
 * The body of a team invitation.
 *
 * @module types/inviteuserpayload
 */

import type { InvitePermissionMode } from '~/types/invitepermissionmode'
import type { TeamPermission } from '~/types/teampermission'

/** The fields `POST /api/portal/client/users/invite` needs to invite someone to the account. */
export interface InviteUserPayload {
  /** The invitee's email address. */
  email: string
  /** The invitee's given name. */
  firstName: string
  /** The invitee's family name. */
  lastName: string
  /** Whether the invitee gets every permission or only the ones in {@link permissions}. */
  permissionMode: InvitePermissionMode
  /** The ticked permissions when choosing; empty for `InvitePermissionMode.All`. */
  permissions: TeamPermission[]
}

/**
 * Which client statuses restrict the account, shared by the screens that set a status.
 *
 * Suspended and Closed change what the client and their services can do, so a screen spells out
 * their effects and asks the operator before applying one; Active and Inactive restrict nothing.
 * The status names double as the i18n keys of the effects and confirmation texts.
 */

import { ClientStatus } from '@/types/client'

/** A status that restricts the account. */
export type RestrictiveClientStatus = ClientStatus.Suspended | ClientStatus.Closed

/**
 * Tells whether a status restricts the account.
 *
 * @param value - The status to test.
 * @returns True for Suspended and Closed.
 */
export const restrictsAccount = (value: string): value is RestrictiveClientStatus =>
  value === ClientStatus.Suspended || value === ClientStatus.Closed

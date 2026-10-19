/**
 * Tells a real account record from a sign-in acknowledgement.
 *
 * `POST /api/portal/auth/login` and `/2fa-login` set the session cookies and answer
 * `{ success: true }`; the account itself comes from `GET /client/me`. Storing that
 * acknowledgement as the account made the session look "loaded" with no name and no e-mail, so
 * the header avatar showed `?` and the menu read "Client Area" for the whole session, and
 * `fetchUser` (which skips when a user is present) never fetched the real profile.
 *
 * @module utils/asClientUser
 */

import type { ClientUser } from '~/types/clientuser'

/**
 * Narrows a sign-in answer to an account record.
 *
 * @param answer What the sign-in endpoint answered with.
 * @returns The record when it carries an account id, otherwise `null` so the profile is fetched.
 */
export const asClientUser = (answer: unknown): ClientUser | null => {
  if (typeof answer === 'object' && answer !== null && 'id' in answer) {
    return answer as ClientUser
  }
  return null
}

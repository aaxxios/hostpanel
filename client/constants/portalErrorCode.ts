/**
 * Machine-readable error codes that the portal branches on.
 *
 * Kept apart from `lib/errorHandler.ts` because the Nitro BFF also needs one of them
 * (`accept-invite.post.ts` writes it) and server code must not import the client-side reader,
 * which reaches for the Nuxt app instance to translate its one fallback sentence.
 *
 * @module constants/portalErrorCode
 */

/**
 * Codes the API (or the BFF) sends, that something actually branches on.
 *
 * SCREAMING_SNAKE, matching the backend. These strings cross the wire and are part of the
 * contract: renaming one here without renaming it at its source silently turns the branch off,
 * with no error and no failing type-check. A code that merely needs displaying needs no entry,
 * because displaying it means rendering `detail` and nothing else.
 */
export const PortalErrorCode = {
  /**
   * The signed-in identity has no row in the backend's `clients` table.
   *
   * Not a failure. Staff identities -- the platform superadmin above all -- authenticate fine and
   * were simply never onboarded as customers, so every "my ..." endpoint answers 404 with this
   * code. `stores/client.ts` turns it into a state flag so the portal renders an explanation and
   * a way out instead of a red alert; the explanation itself is the sentence the API sent.
   */
  ClientProfileNotFound: 'CLIENT_PROFILE_NOT_FOUND',

  /**
   * An invitation was opened by someone who is not signed in.
   *
   * Not a failure either, and not an expired session: the invitation mail lands in a mail client,
   * so arriving with no credential is the normal first visit. **This one code is written by the
   * BFF, not by the C# API** (`server/api/portal/auth/accept-invite.post.ts` refuses before any
   * request to the backend is possible), so the backend has no resource entry for it and its
   * sentence stays in `client.acceptInvite.signInRequired`. That is the second client-authored
   * sentence the localization rule allows -- a guard before any request is possible.
   */
  InviteSignInRequired: 'INVITE_SIGN_IN_REQUIRED',
} as const

/** Any of the codes in {@link PortalErrorCode}. */
export type PortalErrorCodeValue = typeof PortalErrorCode[keyof typeof PortalErrorCode]

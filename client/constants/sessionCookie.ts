/**
 * Names of the cookies that carry the sign-in session between the browser and the Nitro routes.
 *
 * One enum so the routes that set a cookie, the ones that read it and the ones that clear it
 * cannot disagree about its name.
 *
 * @module constants/sessionCookie
 */

/** The session cookies the portal sets or reads. */
export enum SessionCookie {
  /**
   * The opaque id of the signed-in session; httpOnly. The tokens it names stay on the server
   * (`server/utils/portal-session.ts`), so this is the only credential the browser holds.
   */
  Session = 'portal_session',
  /** Legacy cookie that carried the raw access token; only ever cleared now. */
  LegacyAuthToken = 'auth_token',
  /**
   * Legacy cookie that carried the raw refresh token; only ever cleared now. A refresh token can be
   * larger than the 4 KB a browser keeps per cookie, so the browser dropped it and the session
   * ended with the access token.
   */
  LegacyRefreshToken = 'refresh_token',
  /** Readable flag (`1`) that tells the browser a session exists without exposing the token. */
  Authed = 'authed',
  /** Marks that the silent single sign-on attempt has been made for this visit. */
  SsoSilentTried = 'sso_silent_tried',
  /** The page to return to once the silent single sign-on attempt finishes. */
  SsoSilentReturn = 'sso_silent_return',
  /** The PKCE code verifier of a sign-in round trip. */
  PkceVerifier = 'pkce_verifier',
  /** The anti-forgery state of a sign-in round trip. */
  OauthState = 'oauth_state',
  /** The page to open after the sign-in round trip completes. */
  PostLoginRedirect = 'post_login_redirect',
  /** Legacy readable cookie that once carried the signed-in account to the widget; only ever cleared now. */
  PendingAccount = 'inno_pending_account'
}

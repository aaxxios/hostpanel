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
  /** Access token; httpOnly, sent to the backend as the bearer. */
  AuthToken = 'auth_token',
  /** Refresh token; httpOnly, exchanged for a new access token on a 401. */
  RefreshToken = 'refresh_token',
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
  PendingAccount = 'inno_pending_account',
  /** The JSON session of a signed-in customer, read by `requireClientSession`. */
  ClientSession = 'client_session'
}

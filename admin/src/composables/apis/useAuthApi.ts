/**
 * One function per endpoint on the API's auth surface. No state lives here — no
 * `ref`, no caching — so this file reads as a list of what the backend offers.
 *
 * Callers are the auth store, never a `.vue` file.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import { LogoutScope } from '@/types/auth'
import type {
  AuthModeResponse,
  LocalLoginResponse,
  MeResponse,
  RegisterResponse,
  SetupRequiredResponse,
  TwoFactorLoginResponse,
} from '@/types/auth'
import { API_BASE, ApiPath } from '@/lib/apiPaths'
import { HttpMethod } from '@/types/http'


/**
 * Endpoint functions for `/api/auth/*`.
 *
 * Every function rejects with an `ApiError` carrying the response body on failure;
 * turning that into words is `lib/errorHandler.ts`'s job, and deciding what to
 * render is the store's.
 *
 * @returns One typed function per auth endpoint the admin panel uses.
 */
export const useAuthApi = () => {
  const { request, probe } = useApi()

  /**
   * Asks how this deployment signs people in. Anonymous, and has to be — a client
   * must read it before it can authenticate.
   *
   * @returns The deployment's auth mode.
   */
  const fetchMode = (): Promise<AuthModeResponse> => request<AuthModeResponse>(`${ApiPath.Auth}/mode`)

  /**
   * Asks whether first-run bootstrap is still outstanding, i.e. whether the Admin
   * role is unclaimed.
   *
   * @returns Whether setup is required.
   */
  const fetchSetupRequired = (): Promise<SetupRequiredResponse> =>
    request<SetupRequiredResponse>(`${ApiPath.Auth}/setup-required`)

  /**
   * Loads the signed-in account. Whether a session exists is a question only the
   * server can answer, in either mode.
   *
   * @returns Email, locally granted roles, and email-verified flag; null when signed out (401).
   */
  const fetchMe = (): Promise<MeResponse | null> => probe<MeResponse>(`${ApiPath.Auth}/me`)

  /**
   * Signs in with email and password. Local mode only — under SSO this same path is
   * a redirect endpoint and the POST answers 404.
   *
   * @param email - The operator's email address.
   * @param password - The operator's password.
   * @returns Either a bearer token, or a 2FA continuation.
   */
  const localLogin = (email: string, password: string): Promise<LocalLoginResponse> =>
    request<LocalLoginResponse>(`${ApiPath.Auth}/login`, {
      method: HttpMethod.Post,
      body: JSON.stringify({ email, password }),
    })

  /**
   * Completes a sign-in that asked for a second factor.
   *
   * @param pendingToken - The continuation token from {@link localLogin}, held in memory.
   * @param code - The digits the operator's authenticator app currently shows.
   * @returns A bearer token for the completed session.
   */
  const twoFactorLogin = (pendingToken: string, code: string): Promise<TwoFactorLoginResponse> =>
    request<TwoFactorLoginResponse>(`${ApiPath.Auth}/2fa-login`, {
      method: HttpMethod.Post,
      body: JSON.stringify({ pendingToken, code }),
    })

  /**
   * Creates a local account. Local mode only — under SSO this answers 404, because the
   * accounts belong to the sign-on service.
   *
   * @param email - Address the account signs in with.
   * @param password - Chosen password. Its rules are the server's; a rejection comes back
   * as the server's own sentence rather than being pre-empted here.
   * @param firstName - Given name.
   * @param lastName - Family name.
   * @returns The identifier of the created account.
   */
  const register = (
    email: string,
    password: string,
    firstName: string,
    lastName: string,
  ): Promise<RegisterResponse> =>
    request<RegisterResponse>(`${ApiPath.Auth}/register`, {
      method: HttpMethod.Post,
      body: JSON.stringify({ email, password, firstName, lastName }),
    })

  /**
   * Claims the Admin role for the caller. Requires an authenticated session and only
   * succeeds while nobody holds Admin — a second caller gets 409.
   *
   * @param setupToken - The token this installation printed to its log, required in local
   * mode and ignored under SSO. Omitted when {@link SetupRequiredResponse.tokenRequired}
   * said none is wanted.
   * @returns Confirmation that the role was granted.
   */
  const claimAdmin = (setupToken?: string): Promise<{ success: boolean }> =>
    request<{ success: boolean }>(`${ApiPath.Auth}/setup`, {
      method: HttpMethod.Post,
      body: JSON.stringify({ setupToken: setupToken ?? null }),
    })

  /**
   * Ends a local-mode session by asking the API to clear the httpOnly session cookie.
   * The page cannot read that cookie, so it cannot delete it either — signing out has to
   * be a request.
   *
   * The verb carries the distinction, the way it already does for login. Under SSO the
   * shared auth package maps `POST /api/auth/logout`; this controller serves `DELETE` on
   * the same path, so the router tells them apart by method and neither shadows the other.
   * Sending `POST` here would reach the SSO's endpoint under one mode and nothing at all
   * under the other.
   *
   * @returns Confirmation that the cookie was cleared.
   */
  const localLogout = (): Promise<{ success: boolean }> =>
    request<{ success: boolean }>(ApiPath.AuthLogout, { method: HttpMethod.Delete })

  /**
   * Confirms an email address with the token mailed to it. Anonymous; local mode only --
   * the API answers 404 where an SSO owns the accounts, and no SSO route exists for it.
   * Resolves on 200; a 400 (`TOKEN_INVALID_OR_EXPIRED`) rejects with an `ApiError`.
   *
   * @param token - Verification token from the mailed link.
   * @param email - Address being verified.
   * @returns Nothing once the API accepted the token.
   */
  const verifyEmail = (token: string, email: string): Promise<void> =>
    request<void>(`${ApiPath.Auth}/confirm-email`, {
      method: HttpMethod.Post,
      body: JSON.stringify({ email, token }),
    })

  /**
   * Asks the API to mail the signed-in account its verification link again. Authenticated,
   * no body; answers 204, also when the address is already confirmed.
   *
   * @returns Nothing once the API accepted the request.
   */
  const resendVerification = (): Promise<void> =>
    request<void>(`${ApiPath.Auth}/resend-verification`, { method: HttpMethod.Post })

  /**
   * Builds the address that sends the browser through the SSO sign-in and back to a page.
   * Meaningful only under `AUTH_MODE=sso`, where `/api/auth/login` is a redirect endpoint;
   * in local mode the same path is a JSON POST endpoint, which is why the login page asks for
   * the mode first.
   *
   * @param returnTo - Path (with query) to come back to after signing in.
   * @returns The same-origin URL to navigate to.
   */
  const ssoLoginUrl = (returnTo: string): string => `${API_BASE}${ApiPath.Auth}/login${buildQuery({ returnTo })}`

  /**
   * Ends the session under SSO — this product's, or every product's with scope 'all' — and
   * asks where the SSO wants the browser sent to finish signing out.
   *
   * @param scope - 'this' ends only Hostpanel's session, 'all' ends every product's.
   * @returns The SSO end-session URL, or null when the API gave none or refused.
   */
  const endSessionUrl = async (scope: LogoutScope = LogoutScope.This): Promise<string | null> => {
    const query = buildQuery({ scope: scope === LogoutScope.All ? scope : undefined })
    const result = await request<{ endSessionUrl?: string } | undefined>(`${ApiPath.AuthLogout}${query}`, {
      method: HttpMethod.Post,
    }).catch(() => undefined)
    return result?.endSessionUrl ?? null
  }

  return {
    fetchMode,
    ssoLoginUrl,
    endSessionUrl,
    verifyEmail,
    resendVerification,
    fetchSetupRequired,
    fetchMe,
    localLogin,
    twoFactorLogin,
    register,
    claimAdmin,
    localLogout,
  }
}

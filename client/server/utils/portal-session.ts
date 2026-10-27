/**
 * The signed-in portal session, kept on the server.
 *
 * The browser holds one small opaque cookie (`SessionCookie.Session`, a random id) and the readable
 * `authed` hint; the access and refresh tokens stay in the session store
 * (`server/utils/session-store.ts`). Tokens used to travel as cookies, and a refresh token from the
 * identity provider can be larger than the 4 KB a browser keeps per cookie: the browser dropped it
 * without a word, and the session ended the moment the access token expired.
 *
 * Every route that needs the person's bearer asks `resolveAccessToken`, which also renews an access
 * token that is about to expire; `internalApiCall` refreshes once more on a 401. Signing in starts a
 * session with a fresh id, signing out ends it, and a back-channel logout ends every session of the
 * person from the API side through the store's subject index.
 *
 * @module server/utils/portal-session
 */

import { randomBytes } from 'node:crypto'
import type { H3Event } from 'h3'
import { SessionCookie } from '~/constants/sessionCookie'
import type { PortalSession } from '~/types/portalsession'
import {
  ACCESS_TOKEN_FALLBACK_MAX_AGE,
  AUTHED_FLAG_MAX_AGE,
  REFRESH_TOKEN_MAX_AGE,
  readTokenExpiry,
  readTokenPayload
} from '~/server/utils/auth-cookie'
import { useSessionStore } from '~/server/utils/session-store'

/** The tokens a sign-in hands over. */
export interface SessionTokens {
  /** The access token. */
  accessToken: string
  /** The refresh token, when the provider issued one. */
  refreshToken?: string
  /** Lifetime of the access token in seconds, when the provider stated it. */
  expiresIn?: number
}

/** A session together with the id it is stored under. */
export interface ActiveSession {
  /** The opaque id from the browser cookie. */
  id: string
  /** The stored session. */
  session: PortalSession
}

/** The SSO token endpoint's reply to a refresh-token grant (OAuth field names). */
interface RefreshTokenResponse {
  /** The new access token. */
  access_token: string
  /** The rotated refresh token, when the SSO issued one. */
  refresh_token?: string
  /** Lifetime of the new access token, in seconds. */
  expires_in?: number
}

/** Random bytes in a session id; 32 bytes is 43 characters of base64url. */
const SESSION_ID_BYTES = 32

/** An access token this close to its expiry, in seconds, is renewed before it is used. */
const REFRESH_LEEWAY_SECONDS = 30

/** Refreshes in flight, by session id, so concurrent requests of one session share one refresh. */
const refreshesInFlight = new Map<string, Promise<string | null>>()

/**
 * The current time in whole seconds since the epoch.
 *
 * @returns The current time.
 */
const nowSeconds = (): number => Math.floor(Date.now() / 1000)

/**
 * How long a session lives: as long as its refresh token can renew it, or, with none, as long as
 * its access token is good (the API's sliding renewal then extends it on every call).
 *
 * @param session - The session.
 * @returns The lifetime in seconds, at least one.
 */
export const sessionLifetime = (session: PortalSession): number => {
  if (session.refreshToken) {
    return REFRESH_TOKEN_MAX_AGE
  }
  if (session.accessTokenExpiresAt !== undefined) {
    return Math.max(1, session.accessTokenExpiresAt - nowSeconds())
  }
  return ACCESS_TOKEN_FALLBACK_MAX_AGE
}

/**
 * Builds the stored form of a set of tokens.
 *
 * @param tokens - The tokens.
 * @param previous - The session being renewed, whose refresh token and owner carry over.
 * @returns The session to store.
 */
const buildSession = (tokens: SessionTokens, previous?: PortalSession): PortalSession => {
  const claims = readTokenPayload(tokens.accessToken)
  const subject = typeof claims?.['sub'] === 'string' && claims['sub'] ? claims['sub'] : previous?.subject
  const expiresAt = readTokenExpiry(tokens.accessToken)
    ?? (tokens.expiresIn !== undefined ? nowSeconds() + tokens.expiresIn : undefined)
  const session: PortalSession = { accessToken: tokens.accessToken }
  const refreshToken = tokens.refreshToken ?? previous?.refreshToken
  if (refreshToken) {
    session.refreshToken = refreshToken
  }
  if (subject) {
    session.subject = subject
  }
  if (expiresAt !== undefined) {
    session.accessTokenExpiresAt = expiresAt
  }
  return session
}

/**
 * Writes the session cookie: httpOnly, Secure in production, SameSite=Lax (the sign-in callback is
 * reached by a top-level redirect from the identity provider, which Strict would not let carry it).
 *
 * @param event - The incoming request.
 * @param id - The opaque session id.
 * @param maxAge - Lifetime in seconds.
 */
const writeSessionCookie = (event: H3Event, id: string, maxAge: number): void => {
  setCookie(event, SessionCookie.Session, id, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    maxAge,
    path: '/',
  })
}

/**
 * Clears every cookie the session is made of, the legacy token cookies included.
 *
 * @param event - The incoming request.
 */
const clearSessionCookies = (event: H3Event): void => {
  deleteCookie(event, SessionCookie.Session, { path: '/' })
  deleteCookie(event, SessionCookie.Authed, { path: '/' })
  deleteCookie(event, SessionCookie.LegacyAuthToken, { path: '/' })
  deleteCookie(event, SessionCookie.LegacyRefreshToken, { path: '/' })
  deleteCookie(event, SessionCookie.PendingAccount, { path: '/' })
}

/**
 * Whether the request carries a session cookie at all. Cheap (no store read): for decisions that
 * only need to know whether someone claims to be signed in, never for access.
 *
 * @param event - The incoming request.
 * @returns True when the session cookie is present.
 */
export const hasSessionCookie = (event: H3Event): boolean => Boolean(getCookie(event, SessionCookie.Session))

/**
 * Reads the session the request's cookie names.
 *
 * @param event - The incoming request.
 * @returns The session and its id, or null when there is no cookie or the store holds no entry for it.
 */
export const readSession = async (event: H3Event): Promise<ActiveSession | null> => {
  const id = getCookie(event, SessionCookie.Session)
  if (!id) {
    return null
  }
  const session = await useSessionStore().get(id)
  return session ? { id, session } : null
}

/**
 * Starts a session for a completed sign-in: stores the tokens under a fresh random id and gives the
 * browser only that id. A session the browser already had is ended first, so an id is never reused
 * across sign-ins.
 *
 * @param event - The incoming request.
 * @param tokens - The tokens the sign-in produced.
 * @returns Resolves once the session is stored and the cookies are set.
 */
export const startSession = async (event: H3Event, tokens: SessionTokens): Promise<void> => {
  const previousId = getCookie(event, SessionCookie.Session)
  if (previousId) {
    await useSessionStore().delete(previousId)
  }

  const id = randomBytes(SESSION_ID_BYTES).toString('base64url')
  const session = buildSession(tokens)
  const lifetime = sessionLifetime(session)
  await useSessionStore().set(id, session, lifetime)

  writeSessionCookie(event, id, lifetime)
  // The readable hint the route middleware checks; not a credential.
  setCookie(event, SessionCookie.Authed, '1', {
    httpOnly: false,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    maxAge: AUTHED_FLAG_MAX_AGE,
    path: '/',
  })
  // Browsers signed in before sessions moved to the server still carry the token cookies.
  deleteCookie(event, SessionCookie.LegacyAuthToken, { path: '/' })
  deleteCookie(event, SessionCookie.LegacyRefreshToken, { path: '/' })
}

/**
 * Ends the session: deletes the stored entry and clears the cookies. Safe to call without one.
 *
 * @param event - The incoming request.
 * @returns Resolves once the entry is gone and the cookies are cleared.
 */
export const endSession = async (event: H3Event): Promise<void> => {
  const id = getCookie(event, SessionCookie.Session)
  clearSessionCookies(event)
  if (id) {
    await useSessionStore().delete(id)
  }
}

/**
 * Replaces the access token of the current session (the API's sliding renewal), keeping the
 * refresh token and the owner.
 *
 * @param event - The incoming request.
 * @param accessToken - The renewed access token.
 * @returns Resolves once the session is updated; nothing happens without a session.
 */
export const updateAccessToken = async (event: H3Event, accessToken: string): Promise<void> => {
  const current = await readSession(event)
  if (!current) {
    return
  }
  const session = buildSession({ accessToken }, current.session)
  const lifetime = sessionLifetime(session)
  await useSessionStore().set(current.id, session, lifetime)
  writeSessionCookie(event, current.id, lifetime)
}

/**
 * Exchanges the stored refresh token for a new access token at the SSO token endpoint.
 *
 * @param event - The incoming request.
 * @param current - The session to renew.
 * @returns The new access token, or null when the session cannot be renewed (it is then ended).
 */
const performRefresh = async (event: H3Event, current: ActiveSession): Promise<string | null> => {
  const refreshToken = current.session.refreshToken
  if (!refreshToken) {
    await endSession(event)
    return null
  }
  try {
    const config = useRuntimeConfig()
    // Bound to a plain `string`: handed a template literal, Nitro tries to match it against its own
    // route table and the inferred reply type recurses (TS2321).
    const tokenUrl: string = `${config.ssoUrl}/connect/token`
    const response = await $fetch<RefreshTokenResponse>(tokenUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: new URLSearchParams({
        grant_type: 'refresh_token',
        refresh_token: refreshToken,
        client_id: config.ssoClientId as string,
        client_secret: config.ssoClientSecret as string,
      }).toString(),
    })
    if (!response?.access_token) {
      await endSession(event)
      return null
    }
    const tokens: SessionTokens = { accessToken: response.access_token }
    if (response.refresh_token) {
      tokens.refreshToken = response.refresh_token
    }
    if (response.expires_in !== undefined) {
      tokens.expiresIn = response.expires_in
    }
    const session = buildSession(tokens, current.session)
    const lifetime = sessionLifetime(session)
    await useSessionStore().set(current.id, session, lifetime)
    writeSessionCookie(event, current.id, lifetime)
    return session.accessToken
  } catch {
    // A refused grant (revoked, expired, already rotated) ends the session: there is nothing left to renew it with.
    await endSession(event)
    return null
  }
}

/**
 * Renews the current session's access token with its refresh token.
 *
 * Concurrent calls for one session share a single exchange, so a rotated refresh token is never
 * presented twice.
 *
 * @param event - The incoming request.
 * @returns The new access token, or null when there is no session or it cannot be renewed (it is then ended).
 */
export const refreshSession = async (event: H3Event): Promise<string | null> => {
  const current = await readSession(event)
  if (!current) {
    await endSession(event)
    return null
  }
  const inFlight = refreshesInFlight.get(current.id)
  if (inFlight) {
    return inFlight
  }
  const refresh = performRefresh(event, current)
  refreshesInFlight.set(current.id, refresh)
  try {
    return await refresh
  } finally {
    refreshesInFlight.delete(current.id)
  }
}

/**
 * The bearer for this request's backend calls: the session's access token, renewed first when it
 * is about to expire and a refresh token can renew it.
 *
 * @param event - The incoming request.
 * @returns The access token, or undefined when the request has no usable session.
 */
export const resolveAccessToken = async (event: H3Event): Promise<string | undefined> => {
  const current = await readSession(event)
  if (!current) {
    return undefined
  }
  const { accessTokenExpiresAt, refreshToken } = current.session
  const expiring = accessTokenExpiresAt !== undefined && accessTokenExpiresAt - REFRESH_LEEWAY_SECONDS <= nowSeconds()
  if (expiring && refreshToken) {
    return (await refreshSession(event)) ?? undefined
  }
  return current.session.accessToken
}

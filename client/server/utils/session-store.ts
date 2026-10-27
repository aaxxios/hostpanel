/**
 * The stores the portal sessions are kept in, and the one the server uses.
 *
 * Redis when `sessionRedis` (`NUXT_SESSION_REDIS`) is set, which every deployment does; an
 * in-process map otherwise, which is right only for a single process (unit tests, the e2e build,
 * `nuxt dev` without the compose stack): it forgets every session on restart.
 *
 * The Redis key layout is a contract with the API: a back-channel logout ends a person's portal
 * sessions from the API side (`PortalSessionRevoker` in the backend) by reading the subject set
 * and deleting the entries it names. Change a key here and there together.
 *
 * - `hostpanel:portal-session:<id>`: the session as JSON, expiring with the session.
 * - `hostpanel:portal-session-subject:<sub>`: a set of the ids of one person's sessions.
 *
 * @module server/utils/session-store
 */

import Redis from 'ioredis'
import type { PortalSession } from '~/types/portalsession'
import type { PortalSessionStore } from '~/types/portalsessionstore'
import { REFRESH_TOKEN_MAX_AGE } from '~/server/utils/auth-cookie'
import { parseRedisConnection } from '~/server/utils/redis-connection'

/** Prefix of the key one session is stored under. */
export const SESSION_KEY_PREFIX = 'hostpanel:portal-session:'

/** Prefix of the key that lists the session ids of one person. */
export const SUBJECT_KEY_PREFIX = 'hostpanel:portal-session-subject:'

/**
 * Key one session is stored under.
 *
 * @param id - The opaque session id.
 * @returns The Redis key.
 */
export const sessionKey = (id: string): string => `${SESSION_KEY_PREFIX}${id}`

/**
 * Key of the set of one person's session ids.
 *
 * @param subject - The person's `sub` claim.
 * @returns The Redis key.
 */
export const subjectKey = (subject: string): string => `${SUBJECT_KEY_PREFIX}${subject}`

/**
 * Reads a stored session back, refusing anything that is not one.
 *
 * @param raw - The stored JSON.
 * @returns The session, or null when the value is missing or malformed.
 */
const decodeSession = (raw: string | null | undefined): PortalSession | null => {
  if (!raw) {
    return null
  }
  try {
    const value = JSON.parse(raw) as Partial<PortalSession>
    return typeof value?.accessToken === 'string' && value.accessToken ? (value as PortalSession) : null
  } catch {
    return null
  }
}

/**
 * A store kept in this process's memory.
 *
 * @param now - Clock in milliseconds; the real one unless a test supplies its own.
 * @returns The store.
 */
export const createMemorySessionStore = (now: () => number = Date.now): PortalSessionStore => {
  /** The entries, each with the moment it expires. */
  const entries = new Map<string, { session: PortalSession, expiresAt: number }>()

  return {
    get: async (id) => {
      const entry = entries.get(id)
      if (!entry) {
        return null
      }
      if (entry.expiresAt <= now()) {
        entries.delete(id)
        return null
      }
      return entry.session
    },
    set: async (id, session, ttlSeconds) => {
      entries.set(id, { session, expiresAt: now() + ttlSeconds * 1000 })
    },
    delete: async (id) => {
      entries.delete(id)
    },
  }
}

/** The part of the Redis client the store uses, so a test can hand in its own. */
export type SessionRedisClient = Pick<Redis, 'get' | 'multi'>

/**
 * A store kept in Redis.
 *
 * @param client - A connected Redis client.
 * @returns The store.
 */
export const createRedisSessionStore = (client: SessionRedisClient): PortalSessionStore => {
  return {
    get: async (id) => decodeSession(await client.get(sessionKey(id))),
    set: async (id, session, ttlSeconds) => {
      const transaction = client.multi().set(sessionKey(id), JSON.stringify(session), 'EX', ttlSeconds)
      if (session.subject) {
        // The set outlives any one session, so it is given the longest lifetime a session can have;
        // an id that outlived its own entry is harmless to a revocation (deleting a missing key).
        transaction.sadd(subjectKey(session.subject), id).expire(subjectKey(session.subject), REFRESH_TOKEN_MAX_AGE)
      }
      await transaction.exec()
    },
    delete: async (id) => {
      const session = decodeSession(await client.get(sessionKey(id)))
      const transaction = client.multi().del(sessionKey(id))
      if (session?.subject) {
        transaction.srem(subjectKey(session.subject), id)
      }
      await transaction.exec()
    },
  }
}

/** The store the server uses, built on first use. */
let activeStore: PortalSessionStore | null = null

/**
 * The store the server uses: Redis when it is configured, memory otherwise.
 *
 * @returns The store, the same instance on every call.
 */
export const useSessionStore = (): PortalSessionStore => {
  if (activeStore) {
    return activeStore
  }
  const options = parseRedisConnection(useRuntimeConfig().sessionRedis as string | undefined)
  if (options) {
    // A short retry per command, so a Redis outage answers a request with an error instead of
    // holding it open until the connection comes back.
    activeStore = createRedisSessionStore(new Redis({ ...options, maxRetriesPerRequest: 2 }))
  } else {
    activeStore = createMemorySessionStore()
  }
  return activeStore
}

/**
 * The stores the portal sessions are kept in, and the one the server uses.
 *
 * Redis when `sessionRedis` (`NUXT_SESSION_REDIS`) is set, which every deployment does; an
 * in-process map otherwise, which is right only for a single process (unit tests, the e2e build,
 * `nuxt dev` without the compose stack): it forgets every session on restart.
 *
 * The sessions hold the person's tokens, so in Redis they are stored encrypted
 * (`server/utils/session-crypto.ts`, key `sessionEncryptionKey`). The key is required whenever
 * Redis is used outside development; building the store without it throws, which the
 * `server/plugins/session-store.ts` plugin turns into a refusal to start.
 *
 * The Redis key layout is a contract with the API: a back-channel logout ends a person's portal
 * sessions from the API side (`PortalSessionRevoker` in the backend) by reading the subject set
 * and deleting the entries it names. The API never reads a session's content. Change a key here
 * and there together.
 *
 * - `hostpanel:portal-session:<id>`: the session (encrypted JSON), expiring with the session.
 * - `hostpanel:portal-session-subject:<sub>`: a set of the ids of one person's sessions.
 * - `hostpanel:portal-session-refresh-lock:<id>`: the refresh lock, held while a token is exchanged.
 *
 * @module server/utils/session-store
 */

import { randomBytes } from 'node:crypto'
import Redis from 'ioredis'
import type { PortalSession } from '~/types/portalsession'
import type { PortalSessionStore } from '~/types/portalsessionstore'
import { REFRESH_TOKEN_MAX_AGE } from '~/server/utils/auth-cookie'
import { parseRedisConnection } from '~/server/utils/redis-connection'
import { decryptSession, encryptSession, readSessionKey } from '~/server/utils/session-crypto'

/** Prefix of the key one session is stored under. */
export const SESSION_KEY_PREFIX = 'hostpanel:portal-session:'

/** Prefix of the key that lists the session ids of one person. */
export const SUBJECT_KEY_PREFIX = 'hostpanel:portal-session-subject:'

/** Prefix of the key one session's refresh lock is held under. */
export const REFRESH_LOCK_KEY_PREFIX = 'hostpanel:portal-session-refresh-lock:'

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
 * Key one session's refresh lock is held under.
 *
 * @param id - The opaque session id.
 * @returns The Redis key.
 */
export const refreshLockKey = (id: string): string => `${REFRESH_LOCK_KEY_PREFIX}${id}`

/** Random bytes in a lock holder's token. */
const LOCK_TOKEN_BYTES = 16

/** Deletes a lock only when its value is the caller's token, atomically. */
const RELEASE_LOCK_SCRIPT = 'if redis.call("get", KEYS[1]) == ARGV[1] then return redis.call("del", KEYS[1]) else return 0 end'

/**
 * Reads a stored session back, refusing anything that is not one.
 *
 * @param raw - The stored value: encrypted when a key is in use, plain JSON otherwise.
 * @param key - The encryption key; null when sessions are stored unencrypted (development).
 * @returns The session, or null when the value is missing, cannot be decrypted or is malformed.
 */
const decodeSession = (raw: string | null | undefined, key: Buffer | null): PortalSession | null => {
  if (!raw) {
    return null
  }
  const json = key ? decryptSession(raw, key) : raw
  if (!json) {
    return null
  }
  try {
    const value = JSON.parse(json) as Partial<PortalSession>
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
  /** The refresh locks, each with its holder and the moment it expires. */
  const locks = new Map<string, { holder: string, expiresAt: number }>()

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
    acquireRefreshLock: async (id, ttlSeconds) => {
      const held = locks.get(id)
      if (held && held.expiresAt > now()) {
        return null
      }
      const holder = randomBytes(LOCK_TOKEN_BYTES).toString('hex')
      locks.set(id, { holder, expiresAt: now() + ttlSeconds * 1000 })
      return holder
    },
    releaseRefreshLock: async (id, holder) => {
      if (locks.get(id)?.holder === holder) {
        locks.delete(id)
      }
    },
  }
}

/** The part of the Redis client the store uses, so a test can hand in its own. */
export type SessionRedisClient = Pick<Redis, 'get' | 'set' | 'eval' | 'multi'>

/**
 * A store kept in Redis.
 *
 * @param client - A connected Redis client.
 * @param encryptionKey - The key sessions are encrypted with; null stores plain JSON (development only).
 * @returns The store.
 */
export const createRedisSessionStore = (client: SessionRedisClient, encryptionKey: Buffer | null = null): PortalSessionStore => {
  return {
    get: async (id) => decodeSession(await client.get(sessionKey(id)), encryptionKey),
    set: async (id, session, ttlSeconds) => {
      const json = JSON.stringify(session)
      const stored = encryptionKey ? encryptSession(json, encryptionKey) : json
      const transaction = client.multi().set(sessionKey(id), stored, 'EX', ttlSeconds)
      if (session.subject) {
        // The set outlives any one session, so it is given the longest lifetime a session can have;
        // an id that outlived its own entry is harmless to a revocation (deleting a missing key).
        transaction.sadd(subjectKey(session.subject), id).expire(subjectKey(session.subject), REFRESH_TOKEN_MAX_AGE)
      }
      await transaction.exec()
    },
    delete: async (id) => {
      const session = decodeSession(await client.get(sessionKey(id)), encryptionKey)
      const transaction = client.multi().del(sessionKey(id))
      if (session?.subject) {
        transaction.srem(subjectKey(session.subject), id)
      }
      await transaction.exec()
    },
    acquireRefreshLock: async (id, ttlSeconds) => {
      const holder = randomBytes(LOCK_TOKEN_BYTES).toString('hex')
      // SET NX with an expiry: one replica wins, and a crashed holder cannot block the session for good.
      const taken = await client.set(refreshLockKey(id), holder, 'EX', ttlSeconds, 'NX')
      return taken === 'OK' ? holder : null
    },
    releaseRefreshLock: async (id, holder) => {
      await client.eval(RELEASE_LOCK_SCRIPT, 1, refreshLockKey(id), holder)
    },
  }
}

/** The store the server uses, built on first use. */
let activeStore: PortalSessionStore | null = null

/**
 * The store the server uses: Redis when it is configured, memory otherwise.
 *
 * @returns The store, the same instance on every call.
 * @throws Error when Redis is configured outside development without a valid session encryption key.
 */
export const useSessionStore = (): PortalSessionStore => {
  if (activeStore) {
    return activeStore
  }
  const config = useRuntimeConfig()
  const options = parseRedisConnection(config.sessionRedis as string | undefined)
  if (options) {
    // Checked before the connection is opened: no session may ever reach Redis unencrypted outside development.
    const encryptionKey = readSessionKey(config.sessionEncryptionKey)
    // A short retry per command, so a Redis outage answers a request with an error instead of
    // holding it open until the connection comes back.
    activeStore = createRedisSessionStore(new Redis({ ...options, maxRetriesPerRequest: 2 }), encryptionKey)
  } else {
    activeStore = createMemorySessionStore()
  }
  return activeStore
}

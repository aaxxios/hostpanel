/**
 * Tests for `server/utils/session-store.ts`: the in-process store expires entries on time, and the
 * Redis store writes the key layout the API's back-channel logout reads (the session under
 * `hostpanel:portal-session:<id>`, its id in the person's `hostpanel:portal-session-subject:<sub>`
 * set). The Redis client is an in-memory stand-in for the few commands the store sends.
 *
 * @module tests/server/utils/session-store.test
 */

import { describe, expect, it, vi } from 'vitest'
import {
  SESSION_KEY_PREFIX,
  SUBJECT_KEY_PREFIX,
  createMemorySessionStore,
  createRedisSessionStore,
  sessionKey,
  subjectKey,
  useSessionStore
} from '~/server/utils/session-store'
import type { SessionRedisClient } from '~/server/utils/session-store'

/** What the stand-in remembers: strings with their TTL, and sets with theirs. */
interface FakeRedisState {
  /** String keys and their values. */
  strings: Map<string, string>
  /** Set keys and their members. */
  sets: Map<string, Set<string>>
  /** The TTL last given to each key, in seconds. */
  ttls: Map<string, number>
}

/**
 * Builds an in-memory stand-in for the Redis commands the store uses (GET, and a MULTI of SET EX,
 * SADD, EXPIRE, DEL and SREM).
 *
 * @returns The client and the state it writes.
 */
const fakeRedis = (): { client: SessionRedisClient, state: FakeRedisState } => {
  const state: FakeRedisState = { strings: new Map(), sets: new Map(), ttls: new Map() }

  /**
   * Starts a transaction whose commands run on `exec`.
   *
   * @returns The chainable transaction.
   */
  const multi = () => {
    const queued: (() => void)[] = []
    const transaction = {
      set: (key: string, value: string, _mode: string, ttl: number) => {
        queued.push(() => {
          state.strings.set(key, value)
          state.ttls.set(key, ttl)
        })
        return transaction
      },
      sadd: (key: string, member: string) => {
        queued.push(() => {
          state.sets.set(key, new Set([...(state.sets.get(key) ?? []), member]))
        })
        return transaction
      },
      expire: (key: string, ttl: number) => {
        queued.push(() => {
          state.ttls.set(key, ttl)
        })
        return transaction
      },
      del: (key: string) => {
        queued.push(() => {
          state.strings.delete(key)
        })
        return transaction
      },
      srem: (key: string, member: string) => {
        queued.push(() => {
          state.sets.get(key)?.delete(member)
        })
        return transaction
      },
      exec: async () => {
        for (const command of queued) {
          command()
        }
        return []
      },
    }
    return transaction
  }

  const client = {
    get: async (key: string) => state.strings.get(key) ?? null,
    multi,
  } as unknown as SessionRedisClient
  return { client, state }
}

describe('session keys', () => {
  it('uses the layout the API reads on a back-channel logout', () => {
    expect(SESSION_KEY_PREFIX).toBe('hostpanel:portal-session:')
    expect(SUBJECT_KEY_PREFIX).toBe('hostpanel:portal-session-subject:')
    expect(sessionKey('abc')).toBe('hostpanel:portal-session:abc')
    expect(subjectKey('person-1')).toBe('hostpanel:portal-session-subject:person-1')
  })
})

describe('createMemorySessionStore', () => {
  it('reads back what it stored until the entry expires', async () => {
    let now = 1_000_000
    const store = createMemorySessionStore(() => now)
    await store.set('s1', { accessToken: 'at' }, 60)

    expect(await store.get('s1')).toEqual({ accessToken: 'at' })
    now += 60_000
    expect(await store.get('s1')).toBeNull()
  })

  it('deletes an entry, and deleting a missing one is not an error', async () => {
    const store = createMemorySessionStore()
    await store.set('s1', { accessToken: 'at' }, 60)

    await store.delete('s1')
    await store.delete('never-there')

    expect(await store.get('s1')).toBeNull()
  })
})

describe('createRedisSessionStore', () => {
  it('stores the session with its TTL and lists its id under the owner', async () => {
    const { client, state } = fakeRedis()
    const store = createRedisSessionStore(client)

    await store.set('s1', { accessToken: 'at', refreshToken: 'rt', subject: 'person-1' }, 900)

    expect(JSON.parse(state.strings.get('hostpanel:portal-session:s1') ?? '{}')).toEqual({ accessToken: 'at', refreshToken: 'rt', subject: 'person-1' })
    expect(state.ttls.get('hostpanel:portal-session:s1')).toBe(900)
    expect([...(state.sets.get('hostpanel:portal-session-subject:person-1') ?? [])]).toEqual(['s1'])
    expect(state.ttls.get('hostpanel:portal-session-subject:person-1')).toBe(30 * 24 * 60 * 60)
    expect(await store.get('s1')).toEqual({ accessToken: 'at', refreshToken: 'rt', subject: 'person-1' })
  })

  it('writes no owner set for a session without a subject', async () => {
    const { client, state } = fakeRedis()

    await createRedisSessionStore(client).set('s1', { accessToken: 'at' }, 60)

    expect(state.sets.size).toBe(0)
  })

  it('deletes the entry and takes its id out of the owner set', async () => {
    const { client, state } = fakeRedis()
    const store = createRedisSessionStore(client)
    await store.set('s1', { accessToken: 'at', subject: 'person-1' }, 60)
    await store.set('s2', { accessToken: 'at2', subject: 'person-1' }, 60)

    await store.delete('s1')

    expect(state.strings.has('hostpanel:portal-session:s1')).toBe(false)
    expect([...(state.sets.get('hostpanel:portal-session-subject:person-1') ?? [])]).toEqual(['s2'])
  })

  it('reads a malformed or token-less entry as no session', async () => {
    const { client, state } = fakeRedis()
    const store = createRedisSessionStore(client)
    state.strings.set('hostpanel:portal-session:bad', 'not json')
    state.strings.set('hostpanel:portal-session:empty', JSON.stringify({ subject: 'x' }))

    expect(await store.get('bad')).toBeNull()
    expect(await store.get('empty')).toBeNull()
    expect(await store.get('missing')).toBeNull()
  })
})

describe('useSessionStore', () => {
  it('falls back to the in-process store when no Redis is configured, and keeps one instance', async () => {
    vi.stubGlobal('useRuntimeConfig', () => ({ sessionRedis: '' }))

    const first = useSessionStore()
    await first.set('s1', { accessToken: 'at' }, 60)

    expect(useSessionStore()).toBe(first)
    expect(await useSessionStore().get('s1')).toEqual({ accessToken: 'at' })
  })
})

/**
 * Tests for the client users store: the list, the search and the membership mutations.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useClientUsersStore } from '@/stores/clientUsers'
import { json, problem, stubFetch, stubOffline } from '../helpers/fetch'

describe('clientUsersStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('fetchUsers returns the list, null with the sentence on refusal', async () => {
    const store = useClientUsersStore()
    stubFetch(json([{ userId: 'u1' }]))
    expect(await store.fetchUsers('7')).toEqual([{ userId: 'u1' }])

    stubFetch(problem('Forbidden.', 403))
    expect(await store.fetchUsers('7')).toBeNull()
    expect(store.error).toBe('Forbidden.')
    expect(store.loading).toBe(false)
  })

  it('searchUsers returns hits and is silent on failure', async () => {
    const store = useClientUsersStore()
    stubFetch(json({ items: [{ id: 'u1', email: 'a@b.c', firstName: 'A', lastName: 'B' }] }))
    expect(await store.searchUsers('a')).toHaveLength(1)

    stubOffline()
    expect(await store.searchUsers('a')).toEqual([])
    expect(store.error).toBeNull()
  })

  it.each([
    ['addUser', (s: ReturnType<typeof useClientUsersStore>) => s.addUser('7', 'u1', 3), 'Could not reach the server. Check your connection and try again.'],
    ['inviteUser', (s: ReturnType<typeof useClientUsersStore>) => s.inviteUser('7', {}), 'Could not reach the server. Check your connection and try again.'],
    ['updatePermissions', (s: ReturnType<typeof useClientUsersStore>) => s.updatePermissions('7', 'u1', 3), 'Could not reach the server. Check your connection and try again.'],
    ['makeOwner', (s: ReturnType<typeof useClientUsersStore>) => s.makeOwner('7', 'u1'), 'Could not reach the server. Check your connection and try again.'],
    ['removeUser', (s: ReturnType<typeof useClientUsersStore>) => s.removeUser('7', 'u1'), 'Could not reach the server. Check your connection and try again.'],
  ])('%s returns true on success and the fallback sentence offline', async (_name, act, fallback) => {
    const store = useClientUsersStore()
    stubFetch(json({}))
    expect(await act(store)).toBe(true)

    stubOffline()
    expect(await act(store)).toBe(false)
    expect(store.error).toBe(fallback)
  })
})

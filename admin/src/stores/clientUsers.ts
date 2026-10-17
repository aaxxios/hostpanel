/**
 * Pinia store for the users linked to one client.
 *
 * Owns every request the client users page makes (list, search, add, invite, permissions,
 * ownership, removal) through `useClientUsersApi`, and the sentence for a refusal, so the
 * page holds no `catch`.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useClientUsersApi } from '@/composables/apis/useClientUsersApi'
import { runAction } from '@/lib/runAction'
import type { ClientUserItem, UserSearchHit } from '@/types/clientuser'

/** Number of suggestions the add-user search offers. */
const SEARCH_RESULT_COUNT = 5

/** Lists, searches and changes the users linked to a client. */
export const useClientUsersStore = defineStore('clientUsers', () => {
  const api = useClientUsersApi()

  /** Sentence for the last failed load or action, null when none. */
  const error = ref<string | null>(null)

  /** Per-field messages of the last refused action, keyed by camelCase field name. */
  const fieldErrors = ref<Record<string, string>>({})

  /** True while the user list is loading. */
  const loading = ref(false)

  /**
   * Loads the users linked to a client.
   *
   * @param clientId - Client id.
   * @returns The users, or null when refused (see `error`).
   */
  const fetchUsers = async (clientId: string): Promise<ClientUserItem[] | null> => {
    loading.value = true
    const result = await runAction(error, () => api.list(clientId))
    loading.value = false
    return result.ok ? result.value : null
  }

  /**
   * Searches Identity users for the add-user panel. A failure is not surfaced: the
   * suggestion list is just empty.
   *
   * @param term - Search text.
   * @returns Up to five matches; empty when the call failed.
   */
  const searchUsers = async (term: string): Promise<UserSearchHit[]> => {
    /** Throwaway error ref: a failed suggestion lookup is deliberately silent. */
    const quiet = ref<string | null>(null)
    const result = await runAction(quiet, () => api.search(term, SEARCH_RESULT_COUNT))
    return result.ok ? result.value.items : []
  }

  /**
   * Runs one mutating request, capturing a refusal into `error`.
   *
   * @param task - The request to run.
   * @returns True when the API accepted it; false when refused.
   */
  const mutate = async (task: () => Promise<void>): Promise<boolean> => {
    const result = await runAction(error, task, fieldErrors)
    return result.ok
  }

  /**
   * Links an existing user to the client.
   *
   * @param clientId - Client id.
   * @param userId - Identity user id.
   * @param permissions - Permission bit-flags.
   * @returns True when added; false when refused (see `error`).
   */
  const addUser = (clientId: string, userId: string, permissions: number): Promise<boolean> =>
    mutate(() => api.add(clientId, userId, permissions))

  /**
   * Invites a new user by email and links them to the client.
   *
   * @param clientId - Client id.
   * @param payload - Email, names and permission bit-flags.
   * @returns True when the invitation was sent; false when refused (see `error`).
   */
  const inviteUser = (clientId: string, payload: Record<string, unknown>): Promise<boolean> =>
    mutate(() => api.invite(clientId, payload))

  /**
   * Replaces a non-owner user permissions on the client.
   *
   * @param clientId - Client id.
   * @param userId - Identity user id.
   * @param permissions - New permission bit-flags.
   * @returns True when updated; false when refused (see `error`).
   */
  const updatePermissions = (clientId: string, userId: string, permissions: number): Promise<boolean> =>
    mutate(() => api.updatePermissions(clientId, userId, permissions))

  /**
   * Transfers client ownership to a linked user.
   *
   * @param clientId - Client id.
   * @param userId - Identity user id of the new owner.
   * @returns True when transferred; false when refused (see `error`).
   */
  const makeOwner = (clientId: string, userId: string): Promise<boolean> =>
    mutate(() => api.makeOwner(clientId, userId))

  /**
   * Unlinks a user from the client.
   *
   * @param clientId - Client id.
   * @param userId - Identity user id.
   * @returns True when removed; false when refused (see `error`).
   */
  const removeUser = (clientId: string, userId: string): Promise<boolean> =>
    mutate(() => api.remove(clientId, userId))

  return {
    error,
    fieldErrors,
    loading,
    fetchUsers,
    searchUsers,
    addUser,
    inviteUser,
    updatePermissions,
    makeOwner,
    removeUser,
  }
})

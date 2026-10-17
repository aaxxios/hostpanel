/**
 * Pinia store for provisioning servers and server groups.
 *
 * Servers and groups are loaded independently; every mutation refreshes the lists it affects.
 * The only caller of `useServersApi`; a refusal is kept as the API's own sentence, never thrown.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useServersApi } from '@/composables/apis/useServersApi'
import { runAction } from '@/lib/runAction'
import { ServerRefresh } from '@/types/server'
import type {
  ServerDto,
  ServerGroupDto,
  ServerGroupPayload,
  ServerPayload,
  TestConnectionResultDto,
} from '@/types/server'

/** Manages all provisioning server and server group state for the admin panel. */
export const useServersStore = defineStore('servers', () => {
  const api = useServersApi()

  /** All provisioning servers. */
  const servers = ref<ServerDto[]>([])

  /** All server groups with their assigned servers. */
  const groups = ref<ServerGroupDto[]>([])

  /** True while a list request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed load, null when none. */
  const error = ref<string | null>(null)

  /** Sentence for the last failed create, update, delete or connection test, null when none. */
  const actionError = ref<string | null>(null)

  /** Per-field sentences of the last refused create or update (camelCase keys), empty when none. */
  const fieldErrors = ref<Record<string, string>>({})

  /**
   * Loads all servers from the API.
   *
   * @returns Resolves when the request is over; a refusal is left in `error`.
   */
  const fetchServers = async (): Promise<void> => {
    loading.value = true
    const result = await runAction(error, api.listServers)
    loading.value = false
    if (result.ok) {
      servers.value = result.value
    }
  }

  /**
   * Loads all server groups from the API.
   *
   * @returns Resolves when the request is over; a refusal is left in `error`.
   */
  const fetchGroups = async (): Promise<void> => {
    loading.value = true
    const result = await runAction(error, api.listServerGroups)
    loading.value = false
    if (result.ok) {
      groups.value = result.value
    }
  }

  /**
   * Runs one mutating request, then refreshes the lists it affects.
   *
   * @param task - The request to run.
   * @param refresh - Which lists to reload after success.
   * @returns True when the API accepted it; false when refused (see `actionError`).
   */
  const mutate = async (task: () => Promise<void>, refresh: ServerRefresh): Promise<boolean> => {
    const result = await runAction(actionError, task, fieldErrors)
    if (result.ok) {
      await (refresh === ServerRefresh.Both ? Promise.all([fetchServers(), fetchGroups()]) : fetchServers())
    }
    return result.ok
  }

  /**
   * Creates a new server and refreshes the servers list.
   *
   * @param payload - Server creation data.
   * @returns True when created; false when refused (see `actionError`).
   */
  const createServer = (payload: ServerPayload): Promise<boolean> =>
    mutate(() => api.createServer(payload), ServerRefresh.Servers)

  /**
   * Updates an existing server and refreshes the servers list.
   *
   * @param id - Server identifier.
   * @param payload - Updated server data.
   * @returns True when updated; false when refused (see `actionError`).
   */
  const updateServer = (id: number, payload: ServerPayload): Promise<boolean> =>
    mutate(() => api.updateServer(id, payload), ServerRefresh.Servers)

  /**
   * Deletes a server and refreshes the servers list.
   *
   * @param id - Server identifier.
   * @returns True when deleted; false when refused (see `actionError`).
   */
  const deleteServer = (id: number): Promise<boolean> =>
    mutate(() => api.deleteServer(id), ServerRefresh.Servers)

  /**
   * Tests the connection to a server and updates its status in the list.
   *
   * @param id - Server identifier.
   * @returns The test result DTO, or null when the request itself was refused (see `actionError`).
   */
  const testConnection = async (id: number): Promise<TestConnectionResultDto | null> => {
    const result = await runAction(actionError, () => api.testServerConnection(id))
    if (!result.ok) {
      return null
    }
    await fetchServers()
    return result.value
  }

  /**
   * Creates a new server group and refreshes both servers and groups.
   *
   * @param payload - Group creation data.
   * @returns True when created; false when refused (see `actionError`).
   */
  const createGroup = (payload: ServerGroupPayload): Promise<boolean> =>
    mutate(() => api.createServerGroup(payload), ServerRefresh.Both)

  /**
   * Updates an existing server group and refreshes both servers and groups.
   *
   * @param id - Group identifier.
   * @param payload - Updated group data.
   * @returns True when updated; false when refused (see `actionError`).
   */
  const updateGroup = (id: number, payload: ServerGroupPayload): Promise<boolean> =>
    mutate(() => api.updateServerGroup(id, payload), ServerRefresh.Both)

  /**
   * Deletes a server group and refreshes both servers and groups.
   *
   * @param id - Group identifier.
   * @returns True when deleted; false when refused (see `actionError`).
   */
  const deleteGroup = (id: number): Promise<boolean> =>
    mutate(() => api.deleteServerGroup(id), ServerRefresh.Both)

  return {
    servers,
    groups,
    loading,
    error,
    actionError,
    fieldErrors,
    fetchServers,
    fetchGroups,
    createServer,
    updateServer,
    deleteServer,
    testConnection,
    createGroup,
    updateGroup,
    deleteGroup,
  }
})

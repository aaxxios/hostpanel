/**
 * Pinia store for managing TLD pricing configurations.
 *
 * Provides CRUD operations, provider import, and price sync for the admin TLD pricing settings
 * page. The only caller of `useTldConfigsApi`; a refusal is kept as the API's own sentence,
 * never thrown.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useTldConfigsApi } from '@/composables/apis/useTldConfigsApi'
import { runAction } from '@/lib/runAction'
import type {
  CreateTldConfigPayload,
  TldConfig,
  TldConfigListItem,
  TldImportResult,
  UpdateTldConfigPayload,
} from '@/types/tldconfig'

/** Manages TLD pricing configuration state and admin CRUD operations. */
export const useTldConfigsStore = defineStore('tldConfigs', () => {
  const api = useTldConfigsApi()

  /** All TLD configs. */
  const configs = ref<TldConfigListItem[]>([])

  /** Whether a fetch is in progress. */
  const loading = ref(false)

  /** Sentence for the last failed load, null when none. */
  const error = ref<string | null>(null)

  /** Sentence for the last failed create, update, delete, import or sync, null when none. */
  const actionError = ref<string | null>(null)

  /**
   * Fetches all TLD configs from the backend.
   *
   * @returns Resolves when the request is over; a refusal is left in `error`.
   */
  const fetchAll = async (): Promise<void> => {
    loading.value = true
    const result = await runAction(error, api.listTldConfigs)
    loading.value = false
    if (result.ok) {
      configs.value = result.value
    }
  }

  /**
   * Fetches a single TLD config by ID.
   *
   * @param id - TLD config identifier.
   * @returns The full TLD config, or undefined when refused (see `error`).
   */
  const fetchById = async (id: number): Promise<TldConfig | undefined> => {
    const result = await runAction(error, () => api.getTldConfig(id))
    return result.ok ? result.value : undefined
  }

  /**
   * Runs one mutating request and refreshes the list when it succeeds.
   *
   * @param task - The request to run.
   * @returns The request's value, or null when refused (see `actionError`).
   */
  const mutate = async <T>(task: () => Promise<T>): Promise<{ value: T } | null> => {
    const result = await runAction(actionError, task)
    if (!result.ok) {
      return null
    }
    await fetchAll()
    return { value: result.value }
  }

  /**
   * Creates a new TLD config and refreshes the list.
   *
   * @param payload - TLD config data.
   * @returns The new config's ID, or null when refused (see `actionError`).
   */
  const create = async (payload: CreateTldConfigPayload): Promise<number | null> => {
    const done = await mutate(() => api.createTldConfig(payload))
    return done ? done.value : null
  }

  /**
   * Updates an existing TLD config and refreshes the list.
   *
   * @param id - TLD config ID.
   * @param payload - Updated TLD config data.
   * @returns True when updated; false when refused (see `actionError`).
   */
  const update = async (id: number, payload: UpdateTldConfigPayload): Promise<boolean> => {
    const done = await mutate(() => api.updateTldConfig(id, payload))
    return done !== null
  }

  /**
   * Deletes a TLD config and refreshes the list.
   *
   * @param id - TLD config ID to delete.
   * @returns True when deleted; false when refused (see `actionError`).
   */
  const remove = async (id: number): Promise<boolean> => {
    const done = await mutate(() => api.deleteTldConfig(id))
    return done !== null
  }

  /**
   * Imports TLDs from a registrar provider and refreshes the list.
   *
   * @param module - Registrar module name, as the backend's registrar module enum spells it.
   * @returns Import result with counts, or null when refused (see `actionError`).
   */
  const importFromProvider = async (module: string): Promise<TldImportResult | null> => {
    const done = await mutate(() => api.importTldConfigs(module))
    return done ? done.value : null
  }

  /**
   * Triggers a manual cost price sync from all providers and refreshes the list.
   *
   * @returns True when synced; false when refused (see `actionError`).
   */
  const syncPrices = async (): Promise<boolean> => {
    const done = await mutate(api.syncTldPrices)
    return done !== null
  }

  return { configs, loading, error, actionError, fetchAll, fetchById, create, update, remove, importFromProvider, syncPrices }
})

/**
 * Pinia store for managing installed plugins.
 *
 * Handles list, install (multipart ZIP), remove, and restart operations. The only caller of
 * `usePluginsApi`; a refusal is kept as the API's own sentence in `error`, never thrown.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { usePluginsApi } from '@/composables/apis/usePluginsApi'
import { runAction } from '@/lib/runAction'
import type { PluginListItemDto } from '@/types/plugin'

/** Holds the installed plugin list and whether a restart is pending. */
export const usePluginsStore = defineStore('plugins', () => {
  const api = usePluginsApi()

  /** Installed plugin list, populated by {@link fetchAll}. */
  const plugins = ref<PluginListItemDto[]>([])

  /** True while any request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed request, null when none. */
  const error = ref<string | null>(null)

  /**
   * Whether a server restart is required for pending plugin changes to take effect.
   * Set to true after a successful install or remove, cleared after restart.
   */
  const requiresRestart = ref(false)

  /**
   * Fetches the list of all installed plugins.
   *
   * @returns Resolves when the request is over; a refusal is left in `error`.
   */
  const fetchAll = async (): Promise<void> => {
    loading.value = true
    const result = await runAction(error, api.listPlugins)
    loading.value = false
    if (result.ok) {
      plugins.value = result.value
    }
  }

  /**
   * Uploads and installs a plugin from a ZIP file, then refreshes the list.
   *
   * @param file - The ZIP file selected by the admin.
   * @returns Resolves when the install request is over; a refusal is left in `error`.
   */
  const install = async (file: File): Promise<void> => {
    loading.value = true
    const result = await runAction(error, () => api.installPlugin(file))
    loading.value = false
    if (result.ok) {
      requiresRestart.value = result.value.requiresRestart
      await fetchAll()
    }
  }

  /**
   * Removes an installed plugin by its identifier.
   *
   * @param id - Plugin identifier as declared in plugin.json.
   * @returns Resolves when the remove request is over; a refusal is left in `error`.
   */
  const remove = async (id: string): Promise<void> => {
    loading.value = true
    const result = await runAction(error, () => api.removePlugin(id))
    loading.value = false
    if (result.ok) {
      requiresRestart.value = result.value.requiresRestart
      plugins.value = plugins.value.filter(p => p.id !== id)
    }
  }

  /**
   * Triggers a graceful server restart so installed/removed plugins take effect.
   * After calling this, poll /api/health until the server responds.
   *
   * @returns Resolves when the restart request is over.
   */
  const restart = async (): Promise<void> => {
    loading.value = true
    /** Local sink: a server that drops the connection while restarting is expected, not an error. */
    const ignored = ref<string | null>(null)
    await runAction(ignored, api.restartServer)
    requiresRestart.value = false
    loading.value = false
  }

  return { plugins, loading, error, requiresRestart, fetchAll, install, remove, restart }
})

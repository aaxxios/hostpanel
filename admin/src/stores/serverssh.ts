/**
 * Pinia store for one server's SSH key management settings: load, save and agent health check.
 * The only caller of the SSH settings endpoints in `useServersApi`; a refusal is kept as the API's
 * own sentence (and per-field sentences), never thrown.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useServersApi } from '@/composables/apis/useServersApi'
import { runAction } from '@/lib/runAction'
import type { ServerSshSettingsDto, ServerSshSettingsPayload, SshAgentTestResultDto } from '@/types/serverssh'

/** SSH settings state for the server whose dialog is open. */
export const useServerSshStore = defineStore('serverSsh', () => {
  const api = useServersApi()

  /** The settings of the open server, once loaded. */
  const settings = ref<ServerSshSettingsDto | null>(null)

  /** True while the settings load. */
  const loading = ref(false)

  /** True while a save is in flight. */
  const saving = ref(false)

  /** True while an agent health check is in flight. */
  const testing = ref(false)

  /** The last health check's outcome in this dialog, null before one ran. */
  const lastTest = ref<SshAgentTestResultDto | null>(null)

  /** Sentence for the last refused request, null when none. */
  const error = ref<string | null>(null)

  /** Per-field sentences of the last refused save. */
  const fieldErrors = ref<Record<string, string>>({})

  /**
   * Loads a server's settings.
   *
   * @param id - Server id.
   * @returns Resolves when settled; a refusal lands in {@link error}.
   */
  const load = async (id: number): Promise<void> => {
    loading.value = true
    const result = await runAction(error, () => api.getServerSshSettings(id))
    settings.value = result.ok ? result.value : null
    loading.value = false
  }

  /**
   * Saves a server's settings.
   *
   * @param id - Server id.
   * @param payload - The settings.
   * @returns True when saved.
   */
  const save = async (id: number, payload: ServerSshSettingsPayload): Promise<boolean> => {
    saving.value = true
    const result = await runAction(error, () => api.saveServerSshSettings(id, payload), fieldErrors)
    if (result.ok) {
      settings.value = result.value
    }
    saving.value = false
    return result.ok
  }

  /**
   * Pings the agent, then reloads the settings so the recorded health shows.
   *
   * @param id - Server id.
   * @returns Resolves when settled.
   */
  const test = async (id: number): Promise<void> => {
    testing.value = true
    const result = await runAction(error, () => api.testServerSshAgent(id))
    lastTest.value = result.ok ? result.value : null
    testing.value = false
    if (result.ok) {
      await load(id)
    }
  }

  /** Clears everything, for a newly opened dialog. */
  const reset = (): void => {
    settings.value = null
    loading.value = false
    saving.value = false
    testing.value = false
    lastTest.value = null
    error.value = null
    fieldErrors.value = {}
  }

  return { settings, loading, saving, testing, lastTest, error, fieldErrors, load, save, test, reset }
})

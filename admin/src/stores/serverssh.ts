/**
 * Pinia store for one server's SSH key management: settings (load, save, agent health check), the
 * installer setup (identity generation, the one-command installer, the network lines) and the host
 * key scan. The only caller of the SSH endpoints in `useServersApi`; a refusal is kept as the API's
 * own sentence (and per-field sentences), never thrown.
 */

import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { useServersApi } from '@/composables/apis/useServersApi'
import { runAction } from '@/lib/runAction'
import { buildInstallCommand, buildNetworkLines } from '@/lib/sshAgent'
import type {
  ServerSshSettingsDto,
  ServerSshSettingsPayload,
  ServerSshSetupDto,
  SshAgentNetworkLines,
  SshAgentTestResultDto,
  SshHostKeyScanDto,
} from '@/types/serverssh'

/** SSH key management state for the server whose dialog is open. */
export const useServerSshStore = defineStore('serverSsh', () => {
  /** The servers endpoints this store is the only caller of. */
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

  /** What the installer needs, once loaded. */
  const setup = ref<ServerSshSetupDto | null>(null)

  /** True while an identity is being generated. */
  const generating = ref(false)

  /** True while a host key scan is in flight. */
  const scanning = ref(false)

  /** The last host key scan in this dialog, null before one ran. */
  const lastScan = ref<SshHostKeyScanDto | null>(null)

  /** Sentence for the last refused request, null when none. */
  const error = ref<string | null>(null)

  /** Why the last host key read failed, shown beside its button; null when it did not fail. */
  const scanError = ref<string | null>(null)

  /** Per-field sentences of the last refused save. */
  const fieldErrors = ref<Record<string, string>>({})

  /**
   * The one-command installer, once an identity exists. It downloads from the HostPanel the
   * administrator is signed in to, so it is built from this page's own origin.
   */
  const installCommand = computed<string | null>(() =>
    (setup.value ? buildInstallCommand(window.location.origin, setup.value) : null))

  /**
   * Firewall and sshd lines that let only this HostPanel reach the saved agent port, null until
   * both the setup and the settings are loaded or while HostPanel's address is not configured.
   */
  const networkLines = computed<SshAgentNetworkLines | null>(() =>
    (setup.value && settings.value ? buildNetworkLines(setup.value.sourceAddresses, settings.value.agentPort) : null))

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

  /**
   * Loads the settings and the installer setup together.
   *
   * @param id - Server id.
   * @returns Resolves when both settled.
   */
  const loadAll = async (id: number): Promise<void> => {
    await load(id)
    const result = await runAction(error, () => api.getServerSshSetup(id))
    setup.value = result.ok ? result.value : null
  }

  /**
   * Generates (or rotates) the server's management identity, then reloads the settings, which
   * rotation switches off.
   *
   * @param id - Server id.
   * @returns True when generated.
   */
  const generateIdentity = async (id: number): Promise<boolean> => {
    generating.value = true
    const result = await runAction(error, () => api.generateServerSshIdentity(id))
    generating.value = false
    if (!result.ok) {
      return false
    }
    setup.value = result.value
    await load(id)
    return true
  }

  /**
   * Scans the host key of the saved agent endpoint.
   *
   * @param id - Server id.
   * @returns Resolves when settled; the result is in {@link lastScan}.
   */
  const scanHostKey = async (id: number): Promise<void> => {
    scanning.value = true
    const result = await runAction(scanError, () => api.scanServerSshHostKey(id))
    lastScan.value = result.ok ? result.value : null
    scanning.value = false
  }

  /** Clears everything, for a newly opened dialog. */
  const reset = (): void => {
    settings.value = null
    loading.value = false
    saving.value = false
    testing.value = false
    lastTest.value = null
    setup.value = null
    generating.value = false
    scanning.value = false
    lastScan.value = null
    error.value = null
    scanError.value = null
    fieldErrors.value = {}
  }

  return {
    settings, setup, loading, saving, testing, generating, scanning, lastTest, lastScan, error, scanError, fieldErrors,
    installCommand, networkLines,
    load, loadAll, save, test, generateIdentity, scanHostKey, reset,
  }
})

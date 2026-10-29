/**
 * SSH keys of the hosting service on screen: the access summary, the key list, and the add and
 * revoke actions with their in-flight flags and the API's refusal sentences. The only caller of
 * the SSH endpoints in `useClientApi`.
 *
 * @module stores/sshkeys
 */
import { defineStore } from 'pinia'
import { useClientApi } from '~/composables/apis/useClientApi'
import { apiErrorMessage } from '~/lib/errorHandler'
import { ActionOutcome } from '~/types/actionoutcome'
import type { SshAccess } from '~/types/sshaccess'
import { SshAccessState } from '~/types/sshaccessstate'
import type { SshKey } from '~/types/sshkey'
import { SshKeyLocalError } from '~/types/sshkeylocalerror'
import { isEd25519PublicKey, looksLikePrivateKey } from '~/utils/sshKey'

/** The SSH key store. */
export const useSshKeysStore = defineStore('sshKeys', () => {
  /** The client endpoints. */
  const api = useClientApi()

  /** The access summary of the service, once loaded. */
  const access = ref<SshAccess | null>(null)
  /** The service's keys, oldest first. */
  const keys = ref<SshKey[]>([])
  /** Whether the summary and list are loading. */
  const loading = ref(false)
  /** The API's sentence when loading failed. */
  const loadError = ref('')
  /** Whether an add is in flight. */
  const adding = ref(false)
  /** The API's sentence when the last add was refused. */
  const addError = ref('')
  /** Why the last add was refused in the browser, before any request. */
  const localError = ref<SshKeyLocalError | null>(null)
  /** The key whose revoke is in flight. */
  const revokingId = ref<string | null>(null)
  /** The API's sentence when the last revoke was refused. */
  const revokeError = ref('')

  /** Whether keys can be added and revoked right now. */
  const canManage = computed(() => access.value?.state === SshAccessState.Available)

  /**
   * Whether listed keys can be revoked: whenever the service is active, even when keys can no
   * longer be added (SSH switched off on the server, the service moved), because removing access never
   * depends on access being available.
   */
  const canRevoke = computed(() => Boolean(access.value) && access.value?.state !== SshAccessState.ServiceInactive)

  /** Whether the service already holds its maximum number of keys. */
  const atLimit = computed(() => Boolean(access.value) && keys.value.length >= (access.value?.maxKeys ?? 0))

  /**
   * Re-reads the key list, keeping the current one if that fails too.
   *
   * @param serviceId - Service id.
   * @returns Resolves when settled.
   */
  const refreshKeys = async (serviceId: string): Promise<void> => {
    try {
      keys.value = await api.fetchSshKeys(serviceId)
    } catch {
      // The action's own refusal is already shown; a second sentence adds nothing.
    }
  }

  /**
   * Loads the access summary, then, unless the service is inactive, the key list.
   *
   * @param serviceId - Service id.
   * @returns Resolves when both settled; a failure lands in {@link loadError}.
   */
  const load = async (serviceId: string): Promise<void> => {
    loading.value = true
    loadError.value = ''
    try {
      access.value = await api.fetchSshAccess(serviceId)
      keys.value = access.value.state === SshAccessState.ServiceInactive ? [] : await api.fetchSshKeys(serviceId)
    } catch (e: unknown) {
      loadError.value = apiErrorMessage(e)
    } finally {
      loading.value = false
    }
  }

  /**
   * Adds a key after the browser-side checks. A private key is refused here and never sent.
   *
   * @param serviceId - Service id.
   * @param label - Friendly label.
   * @param publicKey - The pasted key line.
   * @returns How the attempt ended.
   */
  const add = async (serviceId: string, label: string, publicKey: string): Promise<ActionOutcome> => {
    addError.value = ''
    localError.value = null
    if (looksLikePrivateKey(publicKey)) {
      localError.value = SshKeyLocalError.PrivateKey
      return ActionOutcome.Failed
    }
    if (!isEd25519PublicKey(publicKey)) {
      localError.value = SshKeyLocalError.InvalidKey
      return ActionOutcome.Failed
    }
    if (!label.trim()) {
      localError.value = SshKeyLocalError.LabelRequired
      return ActionOutcome.Failed
    }

    adding.value = true
    try {
      const key = await api.addSshKey(serviceId, { label: label.trim(), publicKey: publicKey.trim() })
      keys.value = [...keys.value, key]
      return ActionOutcome.Succeeded
    } catch (e: unknown) {
      addError.value = apiErrorMessage(e)
      // An unreachable server may leave a failed record behind; show what the backend holds.
      await refreshKeys(serviceId)
      return ActionOutcome.Failed
    } finally {
      adding.value = false
    }
  }

  /**
   * Revokes a key, removing it from the list once the server confirmed.
   *
   * @param serviceId - Service id.
   * @param keyId - The key's public id.
   * @returns How the attempt ended.
   */
  const revoke = async (serviceId: string, keyId: string): Promise<ActionOutcome> => {
    revokeError.value = ''
    revokingId.value = keyId
    try {
      await api.revokeSshKey(serviceId, keyId)
      keys.value = keys.value.filter((key) => key.keyId !== keyId)
      return ActionOutcome.Succeeded
    } catch (e: unknown) {
      revokeError.value = apiErrorMessage(e)
      await refreshKeys(serviceId)
      return ActionOutcome.Failed
    } finally {
      revokingId.value = null
    }
  }

  /** Clears everything, for a newly opened service. */
  const reset = (): void => {
    access.value = null
    keys.value = []
    loading.value = false
    loadError.value = ''
    adding.value = false
    addError.value = ''
    localError.value = null
    revokingId.value = null
    revokeError.value = ''
  }

  return {
    access, keys, loading, loadError, adding, addError, localError, revokingId, revokeError,
    canManage, canRevoke, atLimit, load, add, revoke, reset
  }
})

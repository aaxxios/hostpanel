/**
 * Pinia store for network issues in the admin support module.
 *
 * Owns every request the network issue pages make and the sentence for a refusal, so the
 * pages hold no `catch`.
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useNetworkIssuesApi } from '@/composables/apis/useNetworkIssuesApi'
import { runAction } from '@/lib/runAction'
import { NetworkIssueTab } from '@/types/networkissue'
import type { NetworkIssue, NetworkIssuePayload } from '@/types/networkissue'
import { DEFAULT_PAGE_SIZE } from '@/lib/limits'

/** Paginated, status-filtered listing and CRUD of network issues. */
export const useNetworkIssuesStore = defineStore('networkIssues', () => {
  const api = useNetworkIssuesApi()

  /** Paginated list of network issues. */
  const issues = ref<NetworkIssue[]>([])

  /** Total issue count. */
  const totalCount = ref(0)

  /** Currently loaded issue for editing. */
  const current = ref<NetworkIssue | null>(null)

  /** True while a load or save request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed call, null when none. */
  const error = ref<string | null>(null)

  /** Per-field messages of the last refused save, keyed by camelCase field name. */
  const fieldErrors = ref<Record<string, string>>({})

  /** Active status filter. */
  const statusFilter = ref<NetworkIssueTab>(NetworkIssueTab.Open)

  /** Current page (1-based). */
  const page = ref(1)

  /** Items per page. */
  const pageSize = ref(DEFAULT_PAGE_SIZE)

  /**
   * Loads one page of network issues for the active status filter.
   *
   * @returns True when loaded; false when refused (see `error`).
   */
  const fetchAll = async (): Promise<boolean> => {
    loading.value = true
    const result = await runAction(error, () =>
      api.listNetworkIssues({ page: page.value, pageSize: pageSize.value, status: statusFilter.value }),
    )
    loading.value = false
    if (result.ok) {
      issues.value = result.value.items
      totalCount.value = result.value.totalCount
    }
    return result.ok
  }

  /**
   * Loads one network issue into `current`.
   *
   * @param id - The issue identifier.
   * @returns True when loaded; false when refused (see `error`).
   */
  const fetchById = async (id: number | string): Promise<boolean> => {
    loading.value = true
    current.value = null
    const result = await runAction(error, () => api.getNetworkIssue(id))
    loading.value = false
    if (result.ok) {
      current.value = result.value
    }
    return result.ok
  }

  /**
   * Creates a network issue.
   *
   * @param payload - Issue data.
   * @returns The new issue's id, or null when refused (see `error` and `fieldErrors`).
   */
  const create = async (payload: NetworkIssuePayload): Promise<number | null> => {
    loading.value = true
    const result = await runAction(error, () => api.createNetworkIssue(payload), fieldErrors)
    loading.value = false
    return result.ok ? result.value : null
  }

  /**
   * Replaces a network issue.
   *
   * @param id - The issue identifier.
   * @param payload - Updated data.
   * @returns True when saved; false when refused (see `error` and `fieldErrors`).
   */
  const update = async (id: number | string, payload: NetworkIssuePayload): Promise<boolean> => {
    loading.value = true
    const result = await runAction(error, () => api.updateNetworkIssue(id, payload), fieldErrors)
    loading.value = false
    return result.ok
  }

  /**
   * Deletes a network issue.
   *
   * @param id - The issue identifier.
   * @returns True when deleted; false when refused (see `error`).
   */
  const remove = async (id: number | string): Promise<boolean> =>
    (await runAction(error, () => api.deleteNetworkIssue(id))).ok

  return {
    issues, totalCount, current, loading, error, fieldErrors, statusFilter, page, pageSize,
    fetchAll, fetchById, create, update, remove,
  }
})

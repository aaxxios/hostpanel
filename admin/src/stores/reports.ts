/**
 * Pinia store for the admin reports.
 *
 * Every report screen reads its data through this store: it owns the request, the loading
 * flag and the error sentence, so a report view holds no `try`/`catch` and never touches
 * the transport.
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useReportsApi } from '@/composables/apis/useReportsApi'
import { runAction } from '@/lib/runAction'
import type { ReportClientOption } from '@/types/reportclientoption'

/** Fetches report data and tracks loading and error state for the report screens. */
export const useReportsStore = defineStore('reports', () => {
  const api = useReportsApi()

  /** True while a report is loading. */
  const loading = ref(false)

  /** True while a report refresh (a POST that recomputes the data) is in flight. */
  const updating = ref(false)

  /** Sentence for the last failed report call, null when none. */
  const error = ref<string | null>(null)

  /**
   * Loads one report.
   *
   * @param path - Report endpoint path, including its query string.
   * @returns The report data, or null when refused (see `error`).
   */
  const fetchReport = async <T>(path: string): Promise<T | null> => {
    loading.value = true
    const result = await runAction(error, () => api.fetchReport<T>(path))
    loading.value = false
    return result.ok ? result.value : null
  }

  /**
   * Loads several reports at once; all succeed or none is returned.
   *
   * @param paths - Report endpoint paths, one per result slot.
   * @returns The data in the order of `paths`, or null when any was refused (see `error`).
   */
  const fetchReports = async <T extends unknown[]>(paths: { [K in keyof T]: string }): Promise<T | null> => {
    loading.value = true
    const result = await runAction(error, () =>
      Promise.all((paths as string[]).map(path => api.fetchReport<unknown>(path))),
    )
    loading.value = false
    return result.ok ? (result.value as T) : null
  }

  /**
   * Asks the API to recompute a report and returns the fresh data.
   *
   * @param path - Report endpoint path, including its query string.
   * @returns The refreshed report data, or null when refused (see `error`).
   */
  const refreshReport = async <T>(path: string): Promise<T | null> => {
    updating.value = true
    const result = await runAction(error, () => api.refreshReport<T>(path))
    updating.value = false
    return result.ok ? result.value : null
  }

  /**
   * Loads the client list the report pickers offer. A failure leaves an empty list and the
   * report itself still works, so it is not shown as an error.
   *
   * @returns The clients, or an empty list when the call failed.
   */
  const fetchClientPicker = async (): Promise<ReportClientOption[]> => {
    const failure = ref<string | null>(null)
    const result = await runAction(failure, () => api.fetchClientPicker())
    return result.ok ? result.value : []
  }

  /**
   * Builds the address of a report's CSV export for the page to open.
   *
   * @param path - Export endpoint path, including its query string.
   * @returns The address to open.
   */
  const exportUrl = (path: string): string => api.exportUrl(path)

  return { loading, updating, error, fetchReport, fetchReports, refreshReport, fetchClientPicker, exportUrl }
})

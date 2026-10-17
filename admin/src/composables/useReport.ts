/**
 * Loads one report through the reports store and keeps its data.
 *
 * Every plain report screen did the same: build a path, ask the store, keep the result when the
 * store did not refuse. The store owns the loading flag and the error sentence.
 */
import { onMounted, ref } from 'vue'
import type { Ref } from 'vue'
import { useReportsStore } from '@/stores/reports'

/**
 * Creates the loader of one report.
 *
 * @param path - Builds the endpoint path (with query string) from the screen's current filters.
 * @param immediate - Loads once when the screen mounts; pass false to wait for the operator.
 * @returns The data (null until a load succeeded) and `load`, which fetches it again.
 */
export const useReport = <T>(path: () => string, immediate = true) => {
  const store = useReportsStore()

  /** The report data; null until a load succeeded. */
  const data = ref(null) as Ref<T | null>

  /** Fetches the report with the current filters; a refused load keeps the previous data. */
  const load = async (): Promise<void> => {
    const result = await store.fetchReport<T>(path())
    if (result !== null) {
      data.value = result
    }
  }

  if (immediate) {
    onMounted(load)
  }

  return { data, load }
}

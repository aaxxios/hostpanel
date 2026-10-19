/**
 * Pinia store for the admin-wide billable items list.
 *
 * It is the only caller of `useBillableItemsApi` for this screen; a refusal becomes the API
 * sentence in `error` (and the per-field messages in `fieldErrors`).
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useBillableItemsApi } from '@/composables/apis/useBillableItemsApi'
import { runAction } from '@/lib/runAction'
import type { BillableItem, CreateBillableItemPayload } from '@/types/billableitem'
import { LOOKUP_PAGE_SIZE } from '@/lib/limits'

/** Loads, creates and deletes billable items across all clients. */
export const useBillableItemsStore = defineStore('billableItems', () => {
  const api = useBillableItemsApi()

  /** Loaded billable items. */
  const items = ref<BillableItem[]>([])

  /** True while a request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed request, null when none. */
  const error = ref<string | null>(null)

  /** Per-field messages of the last refused request, keyed by camelCase field name. */
  const fieldErrors = ref<Record<string, string>>({})

  /**
   * Loads the first page of billable items.
   *
   * @param type - Optional type filter (`OneTime` or `Recurring`).
   * @returns Promise that resolves when data is loaded.
   */
  const fetchAll = async (type?: string): Promise<void> => {
    loading.value = true
    const result = await runAction(error, () => api.list(1, LOOKUP_PAGE_SIZE, type), fieldErrors)
    loading.value = false
    if (result.ok) {
      items.value = result.value.items
    }
  }

  /**
   * Creates a billable item, then reloads the list.
   *
   * @param item - Item fields.
   * @returns True when created; false when refused (see `error`).
   */
  const create = async (item: CreateBillableItemPayload): Promise<boolean> => {
    loading.value = true
    const result = await runAction(error, () => api.create(item), fieldErrors)
    loading.value = false
    if (result.ok) {
      await fetchAll()
    }
    return result.ok
  }

  /**
   * Deletes a billable item, then reloads the list.
   *
   * @param id - The item id.
   * @returns True when deleted; false when refused (see `error`).
   */
  const removeItem = async (id: number): Promise<boolean> => {
    loading.value = true
    const result = await runAction(error, () => api.remove(id), fieldErrors)
    loading.value = false
    if (result.ok) {
      await fetchAll()
    }
    return result.ok
  }

  return { items, loading, error, fieldErrors, fetchAll, create, removeItem }
})

/**
 * Pinia store for one client billable items.
 *
 * Owns every request the client billable-items page and its two modals make, through
 * `useBillableItemsApi`, together with the sentence for a refusal, so none of those `.vue`
 * files holds a `catch`.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useBillableItemsApi } from '@/composables/apis/useBillableItemsApi'
import { runAction } from '@/lib/runAction'
import type { BillableItemsResult, ServiceOption } from '@/types/billableitem'

/** Loads, adds, invoices and deletes the billable items of a client. */
export const useClientBillableItemsStore = defineStore('clientBillableItems', () => {
  const api = useBillableItemsApi()

  /** Sentence for the last failed load, invoice or delete, null when none. */
  const error = ref<string | null>(null)

  /** Sentence for the last failed add (single item or time billing), null when none. */
  const saveError = ref<string | null>(null)

  /** Per-field messages of the last refused add, keyed by camelCase field name. */
  const fieldErrors = ref<Record<string, string>>({})

  /** True while the item list is loading. */
  const loading = ref(false)

  /**
   * Loads the uninvoiced items and one page of invoiced items.
   *
   * @param clientId - Client id.
   * @param invoicedPage - Invoiced page, 1-based.
   * @param invoicedPageSize - Invoiced items per page.
   * @returns The items, or null when refused (see `error`).
   */
  const fetchItems = async (
    clientId: string,
    invoicedPage: number,
    invoicedPageSize: number,
  ): Promise<BillableItemsResult | null> => {
    loading.value = true
    const result = await runAction(error, () => api.listByClient(clientId, invoicedPage, invoicedPageSize))
    loading.value = false
    return result.ok ? result.value : null
  }

  /**
   * Invoices the given uninvoiced items.
   *
   * @param clientId - Client id.
   * @param ids - Billable item ids to invoice.
   * @returns True when invoiced; false when refused (see `error`).
   */
  const invoiceSelected = async (clientId: string, ids: number[]): Promise<boolean> => {
    const result = await runAction(error, () => api.invoiceSelected(clientId, ids))
    return result.ok
  }

  /**
   * Deletes billable items one after another; stops at the first refusal.
   *
   * @param clientId - Client id.
   * @param ids - Billable item ids to delete.
   * @returns True when all were deleted; false when one was refused (see `error`).
   */
  const deleteItems = async (clientId: string, ids: number[]): Promise<boolean> => {
    const result = await runAction(error, async () => {
      for (const id of ids) {
        await api.removeForClient(clientId, id)
      }
    })
    return result.ok
  }

  /**
   * Loads the client services for the item dropdowns. A failure leaves the dropdown empty
   * and is not shown, because the form still works without a service.
   *
   * @param clientId - Client id.
   * @returns The options, or an empty list on failure.
   */
  const fetchServiceOptions = async (clientId: string): Promise<ServiceOption[]> => {
    /** Throwaway error ref: the failure is deliberately not surfaced. */
    const quiet = ref<string | null>(null)
    const result = await runAction(quiet, () => api.listServices(clientId))
    return result.ok ? result.value.items.map(s => ({ value: s.id, label: s.productName ?? `#${s.id}` })) : []
  }

  /**
   * Adds one billable item.
   *
   * @param clientId - Client id.
   * @param payload - The item fields.
   * @returns True when saved; false when refused (see `saveError`, `fieldErrors`).
   */
  const addItem = async (clientId: string, payload: Record<string, unknown>): Promise<boolean> => {
    const result = await runAction(saveError, () => api.addForClient(clientId, payload), fieldErrors)
    return result.ok
  }

  /**
   * Adds time-billing entries.
   *
   * @param clientId - Client id.
   * @param entries - The entries to bill.
   * @returns True when saved; false when refused (see `saveError`, `fieldErrors`).
   */
  const addTimeBilling = async (clientId: string, entries: Record<string, unknown>[]): Promise<boolean> => {
    const result = await runAction(saveError, () => api.addTimeBilling(clientId, entries), fieldErrors)
    return result.ok
  }

  return {
    error,
    saveError,
    fieldErrors,
    loading,
    fetchItems,
    invoiceSelected,
    deleteItems,
    fetchServiceOptions,
    addItem,
    addTimeBilling,
  }
})

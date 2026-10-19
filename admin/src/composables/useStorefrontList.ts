/**
 * The delete-confirmation and reorder state of a storefront content list page, over any store
 * built on `useStorefrontCrud`.
 */

import { ref } from 'vue'
import type { Ref } from 'vue'
import type { StorefrontCrud } from '@/composables/useStorefrontCrud'
import type { StorefrontEntity, StorefrontOrderItem } from '@/types/storefront'

/** What {@link useStorefrontList} hands the list view. */
export interface StorefrontList<TItem> {
  /** True while the new order is being saved after a drag. */
  savingOrder: Ref<boolean>
  /** The entry awaiting delete confirmation, null when nothing is staged. */
  deleteTarget: Ref<TItem | null>
  /** True while the staged entry is being deleted. */
  deleting: Ref<boolean>
  /** Opens the delete question for an entry. */
  stageDelete: (item: TItem) => void
  /** Closes the delete question. */
  cancelDelete: () => void
  /** Deletes the staged entry. */
  confirmDelete: () => Promise<void>
  /** Saves a new display order. */
  saveOrder: (order: StorefrontOrderItem[]) => Promise<void>
}

/**
 * Creates the delete and reorder state of a list page.
 *
 * @param store - The content store (its delete and reorder actions).
 * @returns The staged delete, the saving flags and their actions.
 */
export const useStorefrontList = <TItem extends StorefrontEntity>(
  store: Pick<StorefrontCrud<TItem, unknown>, 'remove' | 'updateOrder'>,
): StorefrontList<TItem> => {
  /** True while the new order is being saved after a drag. */
  const savingOrder = ref(false)

  /** The entry awaiting delete confirmation, null when nothing is staged. */
  const deleteTarget = ref<TItem | null>(null) as Ref<TItem | null>

  /** True while the staged entry is being deleted. */
  const deleting = ref(false)

  /**
   * Stages an entry for deletion. Only opens the question; {@link confirmDelete} does the work.
   *
   * @param item - The entry to delete.
   */
  const stageDelete = (item: TItem): void => {
    deleteTarget.value = item
  }

  /** Closes the delete question without deleting. */
  const cancelDelete = (): void => {
    deleteTarget.value = null
  }

  /** Deletes the staged entry and closes the question. */
  const confirmDelete = async (): Promise<void> => {
    const target = deleteTarget.value
    if (!target) {
      return
    }
    deleting.value = true
    await store.remove(target.id)
    deleting.value = false
    deleteTarget.value = null
  }

  /**
   * Saves a new display order.
   *
   * @param order - Entry ID and new sort order pairs.
   */
  const saveOrder = async (order: StorefrontOrderItem[]): Promise<void> => {
    savingOrder.value = true
    await store.updateOrder(order)
    savingOrder.value = false
  }

  return { savingOrder, deleteTarget, deleting, stageDelete, cancelDelete, confirmDelete, saveOrder }
}

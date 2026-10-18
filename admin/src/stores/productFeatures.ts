/**
 * Pinia store for a product's specification lines.
 *
 * The product features modal reads and edits the lines through this store, which owns the
 * requests and the sentence for a refusal, so the modal holds no `catch`.
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useProductFeaturesApi } from '@/composables/apis/useProductFeaturesApi'
import { runAction } from '@/lib/runAction'
import type { ProductFeature } from '@/types/productfeature'

/** Loads, adds, saves and removes the specification lines of a product. */
export const useProductFeaturesStore = defineStore('productFeatures', () => {
  const api = useProductFeaturesApi()

  /** Sentence for the last failed call, null when none. */
  const error = ref<string | null>(null)

  /** Per-field messages of the last refused add or save, keyed by camelCase field name. */
  const fieldErrors = ref<Record<string, string>>({})

  /**
   * Loads a product's lines, newest state from the server.
   *
   * @param productId - Product id.
   * @returns The lines, or null when refused (see `error`).
   */
  const fetchFeatures = async (productId: number): Promise<ProductFeature[] | null> => {
    const result = await runAction(error, () => api.listFeatures(productId))
    return result.ok ? result.value : null
  }

  /**
   * Adds a line.
   *
   * @param productId - Product id.
   * @param label - Row label.
   * @param value - Row value.
   * @param sortOrder - Position of the new line.
   * @returns True when added; false when refused (see `error` and `fieldErrors`).
   */
  const addFeature = async (productId: number, label: string, value: string, sortOrder: number): Promise<boolean> =>
    (await runAction(error, () => api.createFeature({ productId, label, value, sortOrder }), fieldErrors)).ok

  /**
   * Saves an edited line.
   *
   * @param feature - The line with the operator's current text.
   * @returns True when saved; false when refused (see `error` and `fieldErrors`).
   */
  const saveFeature = async (feature: ProductFeature): Promise<boolean> =>
    (await runAction(error, () =>
      api.updateFeature({
        id: feature.id,
        label: feature.label.trim(),
        value: feature.value.trim(),
        sortOrder: feature.sortOrder,
      }),
    fieldErrors)).ok

  /**
   * Removes a line.
   *
   * @param id - Line id.
   * @returns True when removed; false when refused (see `error`).
   */
  const removeFeature = async (id: number): Promise<boolean> =>
    (await runAction(error, () => api.deleteFeature(id))).ok

  return { error, fieldErrors, fetchFeatures, addFeature, saveFeature, removeFeature }
})

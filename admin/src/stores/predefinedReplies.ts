/**
 * Pinia store for predefined replies and their categories.
 *
 * Owns every request the predefined replies page makes and the sentence for a refusal, so the
 * page holds no `catch`.
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { usePredefinedRepliesApi } from '@/composables/apis/usePredefinedRepliesApi'
import { runAction } from '@/lib/runAction'
import type {
  PredefinedReply,
  PredefinedReplyCategory,
  PredefinedReplyPayload,
} from '@/types/predefinedreply'

/** CRUD for reply categories and replies, plus search. */
export const usePredefinedRepliesStore = defineStore('predefinedReplies', () => {
  const api = usePredefinedRepliesApi()

  /** List of reply categories. */
  const categories = ref<PredefinedReplyCategory[]>([])

  /** List of predefined replies. */
  const replies = ref<PredefinedReply[]>([])

  /** True while a load or create request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed call, null when none. */
  const error = ref<string | null>(null)

  /** Per-field messages of the last refused save, keyed by camelCase field name. */
  const fieldErrors = ref<Record<string, string>>({})

  /**
   * Loads every reply category.
   *
   * @returns True when loaded; false when refused (see `error`).
   */
  const fetchCategories = async (): Promise<boolean> => {
    loading.value = true
    const result = await runAction(error, () => api.listCategories())
    loading.value = false
    if (result.ok) {
      categories.value = result.value
    }
    return result.ok
  }

  /**
   * Creates a category, then reloads the list.
   *
   * @param name - The category name.
   * @returns The new category's id, or null when refused (see `error` and `fieldErrors`).
   */
  const createCategory = async (name: string): Promise<number | null> => {
    loading.value = true
    const result = await runAction(error, () => api.createCategory(name), fieldErrors)
    loading.value = false
    if (!result.ok) {
      return null
    }
    await fetchCategories()
    return result.value
  }

  /**
   * Renames a category, then reloads the list.
   *
   * @param id - The category identifier.
   * @param name - The new name.
   * @returns True when renamed; false when refused (see `error` and `fieldErrors`).
   */
  const updateCategory = async (id: number, name: string): Promise<boolean> => {
    const result = await runAction(error, () => api.updateCategory(id, name), fieldErrors)
    if (result.ok) {
      await fetchCategories()
    }
    return result.ok
  }

  /**
   * Deletes a category, then reloads the list.
   *
   * @param id - The category identifier.
   * @returns True when deleted; false when refused (see `error`).
   */
  const deleteCategory = async (id: number): Promise<boolean> => {
    const result = await runAction(error, () => api.deleteCategory(id))
    if (result.ok) {
      await fetchCategories()
    }
    return result.ok
  }

  /**
   * Loads replies, optionally of one category.
   *
   * @param categoryId - Optional category filter.
   * @returns True when loaded; false when refused (see `error`).
   */
  const fetchReplies = async (categoryId?: number): Promise<boolean> => {
    loading.value = true
    const result = await runAction(error, () => api.listReplies(categoryId))
    loading.value = false
    if (result.ok) {
      replies.value = result.value
    }
    return result.ok
  }

  /**
   * Searches replies. The hits are returned, not stored, so a search never replaces the full
   * list the other tabs show.
   *
   * @param query - The search term.
   * @returns The matching replies, or null when the search was refused (see `error`).
   */
  const searchReplies = async (query: string): Promise<PredefinedReply[] | null> => {
    const result = await runAction(error, () => api.searchReplies(query))
    return result.ok ? result.value : null
  }

  /**
   * Creates a reply.
   *
   * @param payload - Reply data.
   * @returns The new reply's id, or null when refused (see `error` and `fieldErrors`).
   */
  const createReply = async (payload: PredefinedReplyPayload): Promise<number | null> => {
    loading.value = true
    const result = await runAction(error, () => api.createReply(payload), fieldErrors)
    loading.value = false
    return result.ok ? result.value : null
  }

  /**
   * Replaces a reply.
   *
   * @param id - The reply identifier.
   * @param payload - Updated data.
   * @returns True when saved; false when refused (see `error` and `fieldErrors`).
   */
  const updateReply = async (id: number, payload: PredefinedReplyPayload): Promise<boolean> =>
    (await runAction(error, () => api.updateReply(id, payload), fieldErrors)).ok

  /**
   * Deletes a reply.
   *
   * @param id - The reply identifier.
   * @returns True when deleted; false when refused (see `error`).
   */
  const deleteReply = async (id: number): Promise<boolean> =>
    (await runAction(error, () => api.deleteReply(id))).ok

  return {
    categories,
    replies,
    loading,
    error,
    fieldErrors,
    fetchCategories,
    createCategory,
    updateCategory,
    deleteCategory,
    fetchReplies,
    searchReplies,
    createReply,
    updateReply,
    deleteReply,
  }
})

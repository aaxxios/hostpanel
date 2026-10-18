/**
 * Endpoints behind the predefined replies store: reply categories and canned replies.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { PredefinedReply, PredefinedReplyCategory, PredefinedReplyPayload } from '@/types/predefinedreply'

/**
 * Endpoint functions for `/predefined-replies`.
 *
 * @returns One typed function per endpoint.
 */
export const usePredefinedRepliesApi = () => {
  const { request } = useApi()

  /**
   * Lists the reply categories.
   *
   * @returns Every category.
   */
  const listCategories = (): Promise<PredefinedReplyCategory[]> =>
    request<PredefinedReplyCategory[]>('/predefined-replies/categories')

  /**
   * Creates a reply category.
   *
   * @param name - Category name.
   * @returns The new category's id.
   */
  const createCategory = (name: string): Promise<number> =>
    request<number>('/predefined-replies/categories', { method: 'POST', body: JSON.stringify({ name }) })

  /**
   * Renames a reply category.
   *
   * @param id - Category id.
   * @param name - The new name.
   * @returns Nothing.
   */
  const updateCategory = (id: number, name: string): Promise<void> =>
    request<void>(`/predefined-replies/categories/${id}`, { method: 'PUT', body: JSON.stringify({ name }) })

  /**
   * Deletes a reply category.
   *
   * @param id - Category id.
   * @returns Nothing.
   */
  const deleteCategory = (id: number): Promise<void> =>
    request<void>(`/predefined-replies/categories/${id}`, { method: 'DELETE' })

  /**
   * Lists replies, optionally of one category.
   *
   * @param categoryId - Category to keep; undefined for all.
   * @returns The replies.
   */
  const listReplies = (categoryId?: number): Promise<PredefinedReply[]> =>
    request<PredefinedReply[]>(`/predefined-replies${buildQuery({ categoryId })}`)

  /**
   * Searches replies by name and content.
   *
   * @param query - The search term.
   * @returns The matching replies.
   */
  const searchReplies = (query: string): Promise<PredefinedReply[]> =>
    request<PredefinedReply[]>(`/predefined-replies/search${buildQuery({ q: query })}`)

  /**
   * Creates a reply.
   *
   * @param payload - Name, content and category.
   * @returns The new reply's id.
   */
  const createReply = (payload: PredefinedReplyPayload): Promise<number> =>
    request<number>('/predefined-replies', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Replaces a reply.
   *
   * @param id - Reply id.
   * @param payload - Name, content and category.
   * @returns Nothing.
   */
  const updateReply = (id: number, payload: PredefinedReplyPayload): Promise<void> =>
    request<void>(`/predefined-replies/${id}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Deletes a reply.
   *
   * @param id - Reply id.
   * @returns Nothing.
   */
  const deleteReply = (id: number): Promise<void> =>
    request<void>(`/predefined-replies/${id}`, { method: 'DELETE' })

  return {
    listCategories, createCategory, updateCategory, deleteCategory,
    listReplies, searchReplies, createReply, updateReply, deleteReply,
  }
}

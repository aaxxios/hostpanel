/**
 * Endpoints behind the knowledge base store: categories and articles.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { useApi } from '@/composables/useApi'
import type { KbArticle, KbArticlePayload, KbCategory, KbCategoryPayload } from '@/types/kbarticle'

/**
 * Endpoint functions for `/admin/knowledgebase`.
 *
 * @returns One typed function per endpoint.
 */
export const useKnowledgebaseApi = () => {
  const { request } = useApi()

  /**
   * Lists the categories with their article counts.
   *
   * @returns Every category.
   */
  const listCategories = (): Promise<KbCategory[]> =>
    request<KbCategory[]>('/admin/knowledgebase/categories')

  /**
   * Creates a category.
   *
   * @param payload - Name, description and hidden flag.
   * @returns The new category's id.
   */
  const createCategory = (payload: KbCategoryPayload): Promise<number> =>
    request<number>('/admin/knowledgebase/categories', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Replaces a category.
   *
   * @param id - Category id.
   * @param payload - Name, description and hidden flag.
   * @returns Nothing.
   */
  const updateCategory = (id: number, payload: KbCategoryPayload): Promise<void> =>
    request<void>(`/admin/knowledgebase/categories/${id}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Deletes a category.
   *
   * @param id - Category id.
   * @returns Nothing.
   */
  const deleteCategory = (id: number): Promise<void> =>
    request<void>(`/admin/knowledgebase/categories/${id}`, { method: 'DELETE' })

  /**
   * Lists every article, unpublished ones included.
   *
   * @returns Every article.
   */
  const listArticles = (): Promise<KbArticle[]> => request<KbArticle[]>('/admin/knowledgebase')

  /**
   * Creates an article.
   *
   * @param payload - Title, content and category name.
   * @returns The new article's id.
   */
  const createArticle = (payload: KbArticlePayload): Promise<number> =>
    request<number>('/admin/knowledgebase', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Deletes an article.
   *
   * @param id - Article id.
   * @returns Nothing.
   */
  const deleteArticle = (id: number): Promise<void> =>
    request<void>(`/admin/knowledgebase/${id}`, { method: 'DELETE' })

  return { listCategories, createCategory, updateCategory, deleteCategory, listArticles, createArticle, deleteArticle }
}

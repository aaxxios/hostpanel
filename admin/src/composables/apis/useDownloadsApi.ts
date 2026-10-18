/**
 * Endpoints behind the downloads store: download categories and downloadable files.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { DownloadCategory, DownloadCategoryPayload } from '@/types/downloadcategory'
import type { DownloadItem, DownloadItemPayload } from '@/types/downloaditem'

/**
 * Endpoint functions for `/downloads`.
 *
 * @returns One typed function per endpoint.
 */
export const useDownloadsApi = () => {
  const { request } = useApi()

  /**
   * Lists every download category.
   *
   * @returns The flat category list.
   */
  const listCategories = (): Promise<DownloadCategory[]> =>
    request<DownloadCategory[]>('/downloads/categories')

  /**
   * Lists downloads, optionally of one category.
   *
   * @param categoryId - Category to keep; null or 0 for all.
   * @returns The downloads.
   */
  const listDownloads = (categoryId: number | null): Promise<DownloadItem[]> =>
    request<DownloadItem[]>(`/downloads${buildQuery({ categoryId: categoryId || undefined })}`)

  /**
   * Creates a download category.
   *
   * @param payload - Name, description, hidden flag and parent.
   * @returns Nothing.
   */
  const createCategory = (payload: DownloadCategoryPayload): Promise<void> =>
    request<void>('/downloads/categories', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Replaces a download category.
   *
   * @param id - Category id.
   * @param payload - Name, description, hidden flag and parent.
   * @returns Nothing.
   */
  const updateCategory = (id: number, payload: DownloadCategoryPayload): Promise<void> =>
    request<void>(`/downloads/categories/${id}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Deletes a download category.
   *
   * @param id - Category id.
   * @returns Nothing.
   */
  const deleteCategory = (id: number): Promise<void> =>
    request<void>(`/downloads/categories/${id}`, { method: 'DELETE' })

  /**
   * Creates a downloadable file entry.
   *
   * @param payload - The download's fields.
   * @returns Nothing.
   */
  const createDownload = (payload: DownloadItemPayload): Promise<void> =>
    request<void>('/downloads', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Replaces a downloadable file entry.
   *
   * @param id - Download id.
   * @param payload - The download's fields.
   * @returns Nothing.
   */
  const updateDownload = (id: number, payload: DownloadItemPayload): Promise<void> =>
    request<void>(`/downloads/${id}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Deletes a downloadable file entry.
   *
   * @param id - Download id.
   * @returns Nothing.
   */
  const deleteDownload = (id: number): Promise<void> =>
    request<void>(`/downloads/${id}`, { method: 'DELETE' })

  return {
    listCategories, listDownloads,
    createCategory, updateCategory, deleteCategory,
    createDownload, updateDownload, deleteDownload,
  }
}

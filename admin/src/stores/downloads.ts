/**
 * Pinia store for the downloads area: download categories and downloadable files.
 *
 * Owns every request the downloads page makes and the sentence for a refusal, so the page
 * holds no `catch`.
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useDownloadsApi } from '@/composables/apis/useDownloadsApi'
import { runAction } from '@/lib/runAction'
import type { DownloadCategory, DownloadCategoryPayload } from '@/types/downloadcategory'
import type { DownloadItem, DownloadItemPayload } from '@/types/downloaditem'

/** What a delete targets. */
export enum DownloadTarget {
  /** A download category. */
  Category = 'category',
  /** A downloadable file. */
  Download = 'download',
}

/** Loads and changes download categories and downloadable files. */
export const useDownloadsStore = defineStore('downloads', () => {
  const api = useDownloadsApi()

  /** All categories (flat list). */
  const categories = ref<DownloadCategory[]>([])

  /** Downloads at the level last loaded. */
  const downloads = ref<DownloadItem[]>([])

  /** Sentence for the last failed call, null when none. */
  const error = ref<string | null>(null)

  /** Per-field messages of the last refused save, keyed by camelCase field name. */
  const fieldErrors = ref<Record<string, string>>({})

  /**
   * Loads every category.
   *
   * @returns True when loaded; false when refused (see `error`).
   */
  const fetchCategories = async (): Promise<boolean> => {
    const result = await runAction(error, () => api.listCategories())
    if (result.ok) {
      categories.value = result.value
    }
    return result.ok
  }

  /**
   * Loads downloads, optionally of one category.
   *
   * @param categoryId - Category to filter by; null or 0 for all.
   * @returns True when loaded; false when refused (see `error`).
   */
  const fetchDownloads = async (categoryId: number | null): Promise<boolean> => {
    const result = await runAction(error, () => api.listDownloads(categoryId))
    if (result.ok) {
      downloads.value = result.value
    }
    return result.ok
  }

  /**
   * Loads categories and downloads together.
   *
   * @param categoryId - Category whose downloads to load; null for all.
   * @returns True when both loaded; false when either was refused (see `error`).
   */
  const fetchAll = async (categoryId: number | null): Promise<boolean> => {
    const [categoriesLoaded, downloadsLoaded] = await Promise.all([fetchCategories(), fetchDownloads(categoryId)])
    return categoriesLoaded && downloadsLoaded
  }

  /**
   * Creates a download category.
   *
   * @param payload - Name, description, hidden flag and parent.
   * @returns True when created; false when refused (see `error` and `fieldErrors`).
   */
  const createCategory = async (payload: DownloadCategoryPayload): Promise<boolean> =>
    (await runAction(error, () => api.createCategory(payload), fieldErrors)).ok

  /**
   * Updates a download category.
   *
   * @param id - Category id.
   * @param payload - Name, description, hidden flag and parent.
   * @returns True when updated; false when refused (see `error` and `fieldErrors`).
   */
  const updateCategory = async (id: number, payload: DownloadCategoryPayload): Promise<boolean> =>
    (await runAction(error, () => api.updateCategory(id, payload), fieldErrors)).ok

  /**
   * Creates a downloadable file entry.
   *
   * @param payload - The download's fields.
   * @returns True when created; false when refused (see `error` and `fieldErrors`).
   */
  const createDownload = async (payload: DownloadItemPayload): Promise<boolean> =>
    (await runAction(error, () => api.createDownload(payload), fieldErrors)).ok

  /**
   * Updates a downloadable file entry.
   *
   * @param id - Download id.
   * @param payload - The download's fields.
   * @returns True when updated; false when refused (see `error` and `fieldErrors`).
   */
  const updateDownload = async (id: number, payload: DownloadItemPayload): Promise<boolean> =>
    (await runAction(error, () => api.updateDownload(id, payload), fieldErrors)).ok

  /**
   * Deletes a category or a download.
   *
   * @param target - Whether a category or a download is deleted.
   * @param id - Its id.
   * @returns True when deleted; false when refused (see `error`).
   */
  const remove = async (target: DownloadTarget, id: number): Promise<boolean> =>
    (await runAction(error, () =>
      target === DownloadTarget.Category ? api.deleteCategory(id) : api.deleteDownload(id),
    )).ok

  return {
    categories, downloads, error, fieldErrors,
    fetchCategories, fetchDownloads, fetchAll,
    createCategory, updateCategory, createDownload, updateDownload, remove,
  }
})

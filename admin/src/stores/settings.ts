/**
 * Pinia store for admin settings, email templates, products and product groups.
 *
 * The only caller of `useSettingsApi` and `useUploadApi` for these screens. A refusal is kept as
 * the API's own sentence in `error`, `actionError` or `uploadError`; no action throws.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useSettingsApi } from '@/composables/apis/useSettingsApi'
import { useUploadApi } from '@/composables/apis/useUploadApi'
import { runAction } from '@/lib/runAction'
import type { EmailTemplate } from '@/types/emailtemplate'
import type { CreateProductPayload, Product, ProductGroup, UpdateProductPayload } from '@/types/product'
import type { Setting } from '@/types/setting'
import type { BrandingImageKind } from '@/types/upload'

/** Holds the settings, email templates, products and groups the settings screens show. */
export const useSettingsStore = defineStore('settings', () => {
  const api = useSettingsApi()
  const { uploadBrandingImage } = useUploadApi()

  /** System settings list. */
  const settings = ref<Setting[]>([])

  /** Email templates list. */
  const emailTemplates = ref<EmailTemplate[]>([])

  /** Products list. */
  const products = ref<Product[]>([])

  /** Product groups list. */
  const productGroups = ref<ProductGroup[]>([])


  /** True while a list request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed load or setting update, null when none. */
  const error = ref<string | null>(null)

  /** Sentence for the last failed product create or update, null when none. */
  const actionError = ref<string | null>(null)

  /** Sentence for the last failed branding upload, null when none. */
  const uploadError = ref<string | null>(null)

  /**
   * Runs one list request under the loading flag and hands its result to the caller on success.
   *
   * @param task - The request to run.
   * @param assign - Receives the result when the request succeeded.
   * @returns Resolves when the request is over; a refusal is left in `error`.
   */
  const load = async <T>(task: () => Promise<T>, assign: (value: T) => void): Promise<void> => {
    loading.value = true
    const result = await runAction(error, task)
    loading.value = false
    if (result.ok) {
      assign(result.value)
    }
  }

  /**
   * Fetches system settings from the backend.
   *
   * @returns Resolves when the request is over.
   */
  const fetchSettings = (): Promise<void> =>
    load(api.listSettings, (rows) => {
      settings.value = rows
    })

  /**
   * Updates a single setting's value and refreshes the list.
   *
   * The backend exposes update-by-id only — there is no create — so a key that
   * was never seeded cannot be added from here. The banner shows the API's own sentence: a
   * refused value names what the key does accept, which a fixed "failed to save" would hide.
   *
   * @param id - Setting ID.
   * @param value - New value to store.
   * @returns True when persisted; false when refused (see `error`).
   */
  const updateSetting = async (id: number, value: string): Promise<boolean> => {
    const result = await runAction(error, () => api.updateSetting(id, value))
    if (result.ok) {
      await fetchSettings()
    }
    return result.ok
  }

  /**
   * Removes a secret setting's stored value and refreshes the list.
   *
   * @param id - Setting ID.
   * @returns True when cleared; false when refused (see `error`).
   */
  const clearSetting = async (id: number): Promise<boolean> => {
    const result = await runAction(error, () => api.clearSetting(id))
    if (result.ok) {
      await fetchSettings()
    }
    return result.ok
  }

  /**
   * Uploads a branding image and returns where it is served from.
   *
   * @param kind - Which branding slot the image is for.
   * @param file - The image file.
   * @returns The stored image's URL, or null when refused (see `uploadError`).
   */
  const uploadBrandingFile = async (kind: BrandingImageKind, file: File): Promise<string | null> => {
    const result = await runAction(uploadError, async () => (await uploadBrandingImage(kind, file)).url)
    return result.ok ? result.value : null
  }

  /**
   * Fetches email templates from the backend.
   *
   * @returns Resolves when the request is over.
   */
  const fetchEmailTemplates = (): Promise<void> =>
    load(api.listEmailTemplates, (rows) => {
      emailTemplates.value = rows
    })

  /**
   * Fetches all products from the backend (including inactive).
   *
   * @returns Resolves when the request is over.
   */
  const fetchProducts = (): Promise<void> =>
    load(api.listProducts, (rows) => {
      products.value = rows
    })

  /**
   * Fetches all product groups from the backend.
   *
   * @returns Resolves when the request is over.
   */
  const fetchProductGroups = (): Promise<void> =>
    load(api.listProductGroups, (rows) => {
      productGroups.value = rows
    })

  /**
   * Creates a new product and refreshes the list.
   *
   * @param payload - Product creation data.
   * @returns The new product ID, or null when refused (see `actionError`).
   */
  const createProduct = async (payload: CreateProductPayload): Promise<number | null> => {
    const result = await runAction(actionError, () => api.createProduct(payload))
    if (!result.ok) {
      return null
    }
    await fetchProducts()
    return result.value
  }

  /**
   * Updates an existing product and refreshes the list.
   *
   * @param id - Product ID.
   * @param payload - Updated product data.
   * @returns True when updated; false when refused (see `actionError`).
   */
  const updateProduct = async (id: number, payload: UpdateProductPayload): Promise<boolean> => {
    const result = await runAction(actionError, () => api.updateProduct(id, payload))
    if (result.ok) {
      await fetchProducts()
    }
    return result.ok
  }

  return {
    settings, emailTemplates, products, productGroups,
    loading, error, actionError, uploadError,
    fetchSettings, updateSetting, clearSetting, fetchEmailTemplates, fetchProducts, fetchProductGroups,
    createProduct, updateProduct, uploadBrandingFile,
  }
})
